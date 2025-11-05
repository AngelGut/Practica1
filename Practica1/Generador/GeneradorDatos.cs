// /Generador/GeneradorDatos.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Practica1.Clases;     
using Practica1.Modelos;    // GestorMatriculas, Curso
using Practica1.Interfeces; // IIdentificable
using Practica1.Atributos; // TipoContrato

namespace Practica1.Generador
{
    /// <summary>
    /// Genera datos de prueba para poblar el sistema.
    /// Cumple: ≥15 estudiantes, ≥5 profesores, ≥10 cursos, ≥30 matrículas, 3–4 calificaciones por matrícula.
    /// </summary>
    public static class GeneradorDatos
    {
        // Generador pseudoaleatorio con semilla fija para reproducibilidad
        private static Random _rnd = new Random(2025);

        internal static string GenerarDatosPrueba(
            Repositorio<Estudiante> repoEst,
            Repositorio<Profesor> repoPro,
            Repositorio<Curso> repoCur,
            GestorMatriculas gestor)
        {
            // Listas base para combinar nombres, apellidos, carreras, departamentos y cursos
            var nombres = new[] { "Ana", "Luis", "María", "Carlos", "Laura", "Jorge", "Sofía", "Pedro", "Elena", "Marco", "Paula", "Iván", "Lucía", "Hugo", "Noelia", "Daniel", "Verónica", "Tomás" };
            var apellidos = new[] { "Mora", "Vega", "Cruz", "Ibarra", "Suárez", "García", "López", "Ramírez", "Paredes", "Núñez", "Rojas", "Castro", "Silva" };

            var carreras = new[] { "Ing. Sistemas", "Ing. Industrial", "Matemática", "Administración", "Contabilidad", "Arquitectura" };
            var departamentos = new[] { "Computación", "Matemática", "Humanidades", "Física", "Economía", "Gestión" };
            var nombresCurso = new[]
            {
                "Programación I","Programación II","Estructuras de Datos","Bases de Datos","Redes",
                "Cálculo I","Cálculo II","Álgebra Lineal","Estadística","Contabilidad I","Finanzas","Física I"
            };

            // ------------------------------------------------------
            // 1) Profesores (≥5)
            // ------------------------------------------------------
            int profesoresCreados = 0;
            for (int i = 0; i < 5; i++)
            {
                var id = $"PRO-{i + 1:D2}";
                var nom = nombres[i % nombres.Length];
                var ape = apellidos[(i + 2) % apellidos.Length];
                var dep = departamentos[i % departamentos.Length];
                var contrato = (TipoContrato)(i % Enum.GetNames(typeof(TipoContrato)).Length);

                // Fecha de nacimiento para asegurar edad ≥25
                var nacimiento = FechaAleatoria(1968, 1990);

                // Salario dentro del rango [500, 10000] para respetar ValidacionRango si lo aplicaste
                var salario = Redondear(_rnd.Next(500, 10001));

                var p = new Profesor(id, nom, ape, nacimiento, dep, contrato, salario);
                SafeAdd(repoPro, p, ref profesoresCreados);
            }

            // ------------------------------------------------------
            // 2) Cursos (≥10) asignando profesor en round-robin
            // ------------------------------------------------------
            var profesores = repoPro.ObtenerTodos().ToList();
            int cursosCreados = 0;
            for (int i = 0; i < 10; i++)
            {
                var codigo = CodigoCurso(i); // p.ej. CS101, CS102, MA101...
                var nombre = nombresCurso[i % nombresCurso.Length];
                var creditos = 3 + (i % 3); // 3,4,5
                var prof = profesores[i % profesores.Count];

                var curso = new Curso(codigo, nombre, creditos, prof);
                SafeAdd(repoCur, curso, ref cursosCreados);
            }

            // ------------------------------------------------------
            // 3) Estudiantes (≥15)
            // ------------------------------------------------------
            int estudiantesCreados = 0;
            for (int i = 0; i < 15; i++)
            {
                var id = $"EST-{i + 1:D3}";
                var nom = nombres[(i * 3) % nombres.Length];
                var ape = apellidos[(i * 5 + 1) % apellidos.Length];
                var carrera = carreras[(i * 2) % carreras.Length];

                // Fecha de nacimiento para edad ≥15
                var nacimiento = FechaAleatoria(1997, 2007);

                // Matrícula con formato ABC-12345
                var matricula = $"{Letras(3)}-{_rnd.Next(10000, 99999)}";

                var e = new Estudiante(id, nom, ape, nacimiento, carrera, matricula);
                SafeAdd(repoEst, e, ref estudiantesCreados);
            }

            // ------------------------------------------------------
            // 4) Matrículas (≥30) sin duplicar Estudiante+Curso
            // ------------------------------------------------------
            var cursos = repoCur.ObtenerTodos().ToList();
            var ests = repoEst.ObtenerTodos().ToList();

            int matriculasCreadas = 0;
            var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // clave: "estId|cursoCod"

            // Generamos pares únicos estudiante-curso hasta llegar a 30 (o más si hay combinaciones)
            while (matriculasCreadas < 30 && usados.Count < ests.Count * cursos.Count)
            {
                var e = ests[_rnd.Next(ests.Count)];
                var c = cursos[_rnd.Next(cursos.Count)];
                var clave = $"{e.Identificacion}|{c.Codigo}";
                if (!usados.Add(clave)) continue; // ya existe esa matrícula

                gestor.MatricularEstudiante(e, c);
                matriculasCreadas++;

                // 5) Registrar 3–4 calificaciones entre 0 y 10 por matrícula recién creada
                var cantNotas = 3 + _rnd.Next(2); // 3 o 4
                for (int k = 0; k < cantNotas; k++)
                {
                    var nota = Redondear(_rnd.NextDouble() * 10); // 0..10 con 2 decimales
                    gestor.AgregarCalificacion(e.Identificacion, c.Codigo, (decimal)nota);
                }
            }

            // Resumen
            return
                $"Profesores: {profesoresCreados}\n" +
                $"Cursos:     {cursosCreados}\n" +
                $"Estudiantes:{estudiantesCreados}\n" +
                $"Matrículas: {matriculasCreadas}";
        }

        // ------------------------------------------------------
        // Helpers internos de generación
        // ------------------------------------------------------

        private static void SafeAdd<T>(Repositorio<T> repo, T item, ref int contador)
            where T : class, IIdentificable
        {
            try
            {
                repo.Agregar(item);
                contador++;
            }
            catch
            {
                // Ignora duplicados si se vuelve a ejecutar el generador
            }
        }

        private static DateTime FechaAleatoria(int anioMin, int anioMax)
        {
            var y = _rnd.Next(anioMin, anioMax + 1);
            var m = _rnd.Next(1, 13);
            var d = _rnd.Next(1, DateTime.DaysInMonth(y, m) + 1);
            return new DateTime(y, m, d);
        }

        private static string Letras(int n)
        {
            var s = new char[n];
            for (int i = 0; i < n; i++) s[i] = (char)('A' + _rnd.Next(0, 26));
            return new string(s);
        }

        private static string CodigoCurso(int i)
        {
            // Alterna prefijos por "área" solo para variedad visual
            var prefix = (i % 3) switch { 0 => "CS", 1 => "MA", _ => "AD" };
            return $"{prefix}{100 + i}";
        }

        private static decimal Redondear(double v) => Math.Round((decimal)v, 2, MidpointRounding.AwayFromZero);
        private static decimal Redondear(int v) => Math.Round((decimal)v, 2, MidpointRounding.AwayFromZero);
    }
}
