using Practica1.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Modelos
{
    // DTOs/records de apoyo para resultados agregados
    //Los records son estructuras inmutables de solo datos, ideales para representar resultados temporales o datos de
    //consulta (como los que devuelven tus métodos LINQ).
    internal record EstudiantePromedio(Estudiante Estudiante, decimal Promedio);
    internal record CursoPopular(Curso Curso, int CantidadEstudiantes);
    internal record EstadisticaCarrera(string Carrera, int Cantidad, decimal PromedioGeneral);

    /// <summary>
    /// Gestor de matrículas con consultas LINQ.
    /// </summary>
    internal class GestorMatriculas
    {
        private readonly List<Matricula> _matriculas = new();
        private static readonly StringComparer IdComparer = StringComparer.OrdinalIgnoreCase;

        // -----------------------------
        // CRUD básico del gestor
        // -----------------------------

        public Matricula MatricularEstudiante(Estudiante estudiante, Curso curso)
        {
            if (estudiante is null) throw new ArgumentNullException(nameof(estudiante));
            if (curso is null) throw new ArgumentNullException(nameof(curso));

            if (ExisteMatricula(estudiante.Identificacion, curso.Codigo))
                throw new InvalidOperationException(
                    $"El estudiante {estudiante.Identificacion} ya está matriculado en el curso {curso.Codigo}.");

            var m = new Matricula(estudiante, curso, DateTime.Today);
            _matriculas.Add(m);
            return m;
        }

        public void AgregarCalificacion(string idEstudiante, string codigoCurso, decimal calificacion)
        {
            if (calificacion < 0m || calificacion > 10m)
                throw new ArgumentOutOfRangeException(nameof(calificacion), "La calificación debe estar entre 0 y 10.");

            var matricula = BuscarMatricula(idEstudiante, codigoCurso)
                ?? throw new InvalidOperationException("No existe la matrícula especificada.");

            matricula.AgregarCalificacion(calificacion);
        }

        public IReadOnlyCollection<Matricula> ObtenerMatriculasPorEstudiante(string idEstudiante)
        {
            if (string.IsNullOrWhiteSpace(idEstudiante)) return Array.Empty<Matricula>();
            return _matriculas
                .Where(m => IdComparer.Equals(m.Estudiante.Identificacion, idEstudiante))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection<Estudiante> ObtenerEstudiantesPorCurso(string codigoCurso)
        {
            if (string.IsNullOrWhiteSpace(codigoCurso)) return Array.Empty<Estudiante>();

            return _matriculas
                .Where(m => IdComparer.Equals(m.Curso.Codigo, codigoCurso)) // filtra por curso
                .Select(m => m.Estudiante)                                   // proyecta a estudiante
                .GroupBy(e => e.Identificacion, IdComparer)                  // elimina duplicados por ID
                .Select(g => g.First())
                .ToList()
                .AsReadOnly();
        }

        public string GenerarReporteEstudiante(string idEstudiante)
        {
            var mats = ObtenerMatriculasPorEstudiante(idEstudiante);
            if (mats.Count == 0) return $"No hay matrículas para el estudiante {idEstudiante}.";

            var est = mats.First().Estudiante;
            var sb = new StringBuilder();
            sb.AppendLine($"Reporte de {est.Nombre} {est.Apellido} (ID: {est.Identificacion})");
            sb.AppendLine(new string('-', 60));

            foreach (var m in mats)
            {
                var promedio = m.ObtenerPromedio();
                sb.AppendLine($"{m.Curso.Codigo} - {m.Curso.Nombre} | Promedio: {promedio:0.00} | Estado: {m.ObtenerEstado()}");
            }

            return sb.ToString();
        }

        // ============================
        // CONSULTAS LINQ DEL PUNTO 8
        // ============================

        // 1) Los 10 estudiantes con mejor promedio general (promedio entre todas sus matrículas)
        public IReadOnlyList<EstudiantePromedio> ObtenerTop10Estudiantes()
        {
            // Agrupa por estudiante y promedia sus promedios de matrícula
            var query = _matriculas
                .GroupBy(m => m.Estudiante, // clave: objeto Estudiante
                           (est, mats) => new EstudiantePromedio(
                               est,
                               mats.Average(x => x.ObtenerPromedio())
                           ))
                .OrderByDescending(ep => ep.Promedio)
                .ThenBy(ep => ep.Estudiante.Apellido) // desempate estable
                .Take(10)
                .ToList();

            return query;
        }

        // 2) Estudiantes con promedio general < 7.0 (en riesgo)
        public IReadOnlyCollection<Estudiante> ObtenerEstudiantesEnRiesgo(decimal umbral = 7.0m)
        {
            // Calcula promedio por estudiante y filtra
            var query = _matriculas
                .GroupBy(m => m.Estudiante)
                .Select(g => new
                {
                    Estudiante = g.Key,
                    Promedio = g.Average(x => x.ObtenerPromedio())
                })
                .Where(x => x.Promedio < umbral)
                .Select(x => x.Estudiante)
                .Distinct()
                .ToList()
                .AsReadOnly();

            return query;
        }

        // 3) Cursos ordenados por cantidad de estudiantes (populares)
        public IReadOnlyList<CursoPopular> ObtenerCursosMasPopulares()
        {
            var query = _matriculas
                .GroupBy(m => m.Curso)
                .Select(g => new CursoPopular(
                    g.Key,
                    // cuenta estudiantes únicos por curso
                    g.Select(m => m.Estudiante.Identificacion).Distinct(IdComparer).Count()
                ))
                .OrderByDescending(cp => cp.CantidadEstudiantes)
                .ThenBy(cp => cp.Curso.Nombre)
                .ToList();

            return query;
        }

        // 4) Promedio general de todos los estudiantes
        //    (promedio de promedios por estudiante; si no hay datos, devuelve 0)
        public decimal ObtenerPromedioGeneral()
        {
            var promediosPorEstudiante = _matriculas
                .GroupBy(m => m.Estudiante)
                .Select(g => g.Average(x => x.ObtenerPromedio()))
                .ToList();

            if (promediosPorEstudiante.Count == 0) return 0m;
            return Math.Round(promediosPorEstudiante.Average(), 2, MidpointRounding.AwayFromZero);
        }

        // 5) Agrupar estudiantes por carrera y mostrar: cantidad y promedio general por carrera
        public IReadOnlyList<EstadisticaCarrera> ObtenerEstadisticasPorCarrera()
        {
            var query = _matriculas
                .GroupBy(m => m.Estudiante) // primero por estudiante
                .Select(g => new { Estudiante = g.Key, Promedio = g.Average(x => x.ObtenerPromedio()) })
                .GroupBy(x => x.Estudiante.Carrera) // luego por carrera
                .Select(g => new EstadisticaCarrera(
                    Carrera: g.Key ?? "(Sin carrera)",
                    Cantidad: g.Count(),
                    PromedioGeneral: Math.Round(g.Average(x => x.Promedio), 2, MidpointRounding.AwayFromZero)
                ))
                .OrderByDescending(e => e.PromedioGeneral)
                .ThenBy(e => e.Carrera)
                .ToList();

            return query;
        }

        // 6) Búsqueda flexible con predicado (requisito)
        public IEnumerable<Estudiante> BuscarEstudiantes(Func<Estudiante, bool> criterio)
        {
            if (criterio is null) throw new ArgumentNullException(nameof(criterio));

            // Tomamos estudiantes únicos de todas las matrículas y aplicamos el predicado
            return _matriculas
                .Select(m => m.Estudiante)
                .GroupBy(e => e.Identificacion, IdComparer)
                .Select(g => g.First())
                .Where(criterio);
        }

        // ---------------------------------------------
        // EXTRA: 3+ lambdas para filtrado/ordenamiento
        // ---------------------------------------------

        // A) Buscar cursos con un criterio flexible
        public IEnumerable<Curso> BuscarCursos(Func<Curso, bool> criterio)
        {
            if (criterio is null) throw new ArgumentNullException(nameof(criterio));
            return _matriculas
                .Select(m => m.Curso)
                .GroupBy(c => c.Codigo, IdComparer)
                .Select(g => g.First())
                .Where(criterio);
        }

        // B) Ordenar estudiantes por una clave arbitraria (ej.: e => e.Apellido, e => e.Carrera)
        public IEnumerable<Estudiante> OrdenarEstudiantes<TKey>(Func<Estudiante, TKey> keySelector, bool descendente = false)
        {
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            var baseSet = _matriculas
                .Select(m => m.Estudiante)
                .GroupBy(e => e.Identificacion, IdComparer)
                .Select(g => g.First());

            return descendente ? baseSet.OrderByDescending(keySelector)
                               : baseSet.OrderBy(keySelector);
        }

        // C) Ordenar cursos por una clave arbitraria (ej.: c => c.Nombre, c => c.Creditos)
        public IEnumerable<Curso> OrdenarCursos<TKey>(Func<Curso, TKey> keySelector, bool descendente = false)
        {
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            var baseSet = _matriculas
                .Select(m => m.Curso)
                .GroupBy(c => c.Codigo, IdComparer)
                .Select(g => g.First());

            return descendente ? baseSet.OrderByDescending(keySelector)
                               : baseSet.OrderBy(keySelector);
        }

        // ============================
        // Helpers internos
        // ============================

        private Matricula? BuscarMatricula(string idEstudiante, string codigoCurso)
            => _matriculas.FirstOrDefault(m =>
                   IdComparer.Equals(m.Estudiante.Identificacion, idEstudiante) &&
                   IdComparer.Equals(m.Curso.Codigo, codigoCurso));

        private bool ExisteMatricula(string idEstudiante, string codigoCurso)
            => BuscarMatricula(idEstudiante, codigoCurso) is not null;
    }
}

