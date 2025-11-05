using System;
using System.Globalization;
using System.Linq;
using Practica1.Atributos;
using Practica1.Clases;        // Estudiante, Profesor, TipoContrato
using Practica1.Modelos;     // Curso, Matricula, GestorMatriculas, Validador, AnalizadorReflection
using Practica1.Servicios;  // Json


namespace Practica1
{
    internal class Program
    {
        // Repositorios en memoria
        private static readonly Repositorio<Estudiante> RepoEstudiantes = new();
        private static readonly Repositorio<Profesor> RepoProfesores = new();
        private static readonly Repositorio<Curso> RepoCursos = new();

        // Gestor de matrículas
        private static readonly GestorMatriculas Gestor = new();

        static void Main()
        {
            // Configuración inicial
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            Logger.Info("Aplicación iniciada"); // <-- 🔹 Aquí, al comenzar

            try
            {
                CargarTodo(); // ← intenta cargar desde JSON
                if (RepoEstudiantes.ObtenerTodos().Count == 0 &&
                    RepoProfesores.ObtenerTodos().Count == 0 &&
                    RepoCursos.ObtenerTodos().Count == 0)
                {
                    SembrarDatosIniciales();
                    Logger.Info("Datos iniciales sembrados");
                }

                // Bucle principal del menú
                while (true)
                {
                    Console.Clear();
                    Titulo("Sistema Académico - Menú Principal");
                    Console.WriteLine("1) Gestionar Estudiantes");
                    Console.WriteLine("2) Gestionar Profesores");
                    Console.WriteLine("3) Gestionar Cursos");
                    Console.WriteLine("4) Matricular Estudiante");
                    Console.WriteLine("5) Registrar Calificaciones");
                    Console.WriteLine("6) Ver Reportes");
                    Console.WriteLine("7) Análisis con Reflection");
                    Console.WriteLine("8) Salir\n");

                    var op = LeerOpcion("Seleccione una opción", 1, 8);

                    switch (op)
                    {
                        case 1:
                            Logger.Info("Entrando al menú de Estudiantes");
                            MenuEstudiantes();
                            break;

                        case 2:
                            Logger.Info("Entrando al menú de Profesores");
                            MenuProfesores();
                            break;

                        case 3:
                            Logger.Info("Entrando al menú de Cursos");
                            MenuCursos();
                            break;

                        case 4:
                            Logger.Info("Matricular estudiante en curso");
                            MenuMatricular();
                            break;

                        case 5:
                            Logger.Info("Registrar calificaciones");
                            MenuCalificaciones();
                            break;

                        case 6:
                            Logger.Info("Generar reportes y estadísticas");
                            MenuReportes();
                            break;

                        case 7:
                            Logger.Info("Ejecutar análisis con Reflection");
                            MenuReflection();
                            break;

                        case 8:
                            Logger.Info("Guardando estado antes de salir");
                            GuardarTodo();
                            Logger.Info("Aplicación finalizada correctamente");
                            Informar("Gracias por usar el sistema. ¡Hasta pronto!");
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Excepción no controlada en Main", "Main", ex);
                Error($"Ocurrió un error: {ex.Message}");
                Pausa();
            }
        }

        // ============================
        // MENÚ: ESTUDIANTES
        // ============================
        private static void MenuEstudiantes()
        {
            while (true)
            {
                Console.Clear();
                Titulo("Gestión de Estudiantes");
                Console.WriteLine("1) Agregar");
                Console.WriteLine("2) Listar");
                Console.WriteLine("3) Buscar");
                Console.WriteLine("4) Modificar");
                Console.WriteLine("5) Eliminar");
                Console.WriteLine("6) Volver");
                Console.WriteLine();

                var op = LeerOpcion("Opción", 1, 6);
                if (op == 6) return;

                try
                {
                    switch (op)
                    {
                        case 1:
                            var est = CrearEstudianteInteractivo();
                            var errores = Validador.Validar(est);
                            if (errores.Any())
                            {
                                foreach (var e in errores) Error(e);
                                Pausa();
                                break;
                            }
                            RepoEstudiantes.Agregar(est);
                            Exito("Estudiante agregado.");
                            Pausa();
                            break;

                        case 2:
                            ListarEstudiantes();
                            Pausa();
                            break;

                        case 3:
                            var idB = LeerNoVacio("ID del estudiante a buscar");
                            var busc = RepoEstudiantes.BuscarPorId(idB);
                            if (busc is null) Aviso("No encontrado.");
                            else Console.WriteLine(busc);
                            Pausa();
                            break;

                        case 4:
                            var idM = LeerNoVacio("ID del estudiante a modificar");
                            var mod = RepoEstudiantes.BuscarPorId(idM);
                            if (mod is null) { Aviso("No encontrado."); Pausa(); break; }

                            Console.WriteLine($"Actual: {mod}");
                            var nuevoNombre = LeerOpcional($"Nombre ({mod.Nombre})");
                            var nuevoApellido = LeerOpcional($"Apellido ({mod.Apellido})");
                            var nuevaCarrera = LeerOpcional($"Carrera ({mod.Carrera})");
                            var nuevaMatricula = LeerOpcional($"N° Matrícula ({mod.NumeroMatricula})");

                            // Actualizamos solo si ingresó algo
                            if (!string.IsNullOrWhiteSpace(nuevoNombre)) mod = new Estudiante(mod.Identificacion, nuevoNombre, mod.Apellido, mod.FechaNacimiento, mod.Carrera, mod.NumeroMatricula);
                            if (!string.IsNullOrWhiteSpace(nuevoApellido)) mod = new Estudiante(mod.Identificacion, mod.Nombre, nuevoApellido, mod.FechaNacimiento, mod.Carrera, mod.NumeroMatricula);
                            if (!string.IsNullOrWhiteSpace(nuevaCarrera)) mod = new Estudiante(mod.Identificacion, mod.Nombre, mod.Apellido, mod.FechaNacimiento, nuevaCarrera, mod.NumeroMatricula);
                            if (!string.IsNullOrWhiteSpace(nuevaMatricula)) mod = new Estudiante(mod.Identificacion, mod.Nombre, mod.Apellido, mod.FechaNacimiento, mod.Carrera, nuevaMatricula);

                            // Reemplazo simple: eliminar y volver a agregar
                            RepoEstudiantes.Eliminar(idM);
                            RepoEstudiantes.Agregar(mod);
                            Exito("Estudiante modificado.");
                            Pausa();
                            break;

                        case 5:
                            var idE = LeerNoVacio("ID del estudiante a eliminar");
                            Console.Write(RepoEstudiantes.Eliminar(idE) ? "Eliminado." : "No existe.");
                            Console.WriteLine();
                            Pausa();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    Pausa();
                }
            }
        }

        // ============================
        // MENÚ: PROFESORES
        // ============================
        private static void MenuProfesores()
        {
            while (true)
            {
                Console.Clear();
                Titulo("Gestión de Profesores");
                Console.WriteLine("1) Agregar");
                Console.WriteLine("2) Listar");
                Console.WriteLine("3) Buscar");
                Console.WriteLine("4) Modificar");
                Console.WriteLine("5) Eliminar");
                Console.WriteLine("6) Volver");
                Console.WriteLine();

                var op = LeerOpcion("Opción", 1, 6);
                if (op == 6) return;

                try
                {
                    switch (op)
                    {
                        case 1:
                            var prof = CrearProfesorInteractivo();
                            var errores = Validador.Validar(prof);
                            if (errores.Any())
                            {
                                foreach (var e in errores) Error(e);
                                Pausa();
                                break;
                            }
                            RepoProfesores.Agregar(prof);
                            Exito("Profesor agregado.");
                            Pausa();
                            break;

                        case 2:
                            ListarProfesores();
                            Pausa();
                            break;

                        case 3:
                            var idB = LeerNoVacio("ID del profesor a buscar");
                            var busc = RepoProfesores.BuscarPorId(idB);
                            if (busc is null) Aviso("No encontrado.");
                            else Console.WriteLine(busc);
                            Pausa();
                            break;

                        case 4:
                            var idM = LeerNoVacio("ID del profesor a modificar");
                            var mod = RepoProfesores.BuscarPorId(idM);
                            if (mod is null) { Aviso("No encontrado."); Pausa(); break; }

                            Console.WriteLine($"Actual: {mod}");
                            var nuevoNombre = LeerOpcional($"Nombre ({mod.Nombre})");
                            var nuevoApellido = LeerOpcional($"Apellido ({mod.Apellido})");
                            var nuevoDepto = LeerOpcional($"Departamento ({mod.Departamento})");
                            var nuevoContrato = LeerOpcional($"Tipo Contrato ({mod.TipoContrato}) [TiempoCompleto/MedioTiempo/Temporal/Honorarios]");
                            var nuevoSalario = LeerDecimalOpcional($"Salario Base ({mod.SalarioBase})");

                            var nombreFinal = string.IsNullOrWhiteSpace(nuevoNombre) ? mod.Nombre : nuevoNombre;
                            var apellidoFinal = string.IsNullOrWhiteSpace(nuevoApellido) ? mod.Apellido : nuevoApellido;
                            var deptoFinal = string.IsNullOrWhiteSpace(nuevoDepto) ? mod.Departamento : nuevoDepto;
                            var contratoFinal = string.IsNullOrWhiteSpace(nuevoContrato) ? mod.TipoContrato :
                                Enum.Parse<TipoContrato>(nuevoContrato, true);
                            var salarioFinal = nuevoSalario ?? mod.SalarioBase;

                            var modificado = new Profesor(mod.Identificacion, nombreFinal, apellidoFinal, mod.FechaNacimiento,
                                                          deptoFinal, contratoFinal, salarioFinal);

                            RepoProfesores.Eliminar(idM);
                            RepoProfesores.Agregar(modificado);
                            Exito("Profesor modificado.");
                            Pausa();
                            break;

                        case 5:
                            var idE = LeerNoVacio("ID del profesor a eliminar");
                            Console.Write(RepoProfesores.Eliminar(idE) ? "Eliminado." : "No existe.");
                            Console.WriteLine();
                            Pausa();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    Pausa();
                }
            }
        }

        // ============================
        // MENÚ: CURSOS
        // ============================
        private static void MenuCursos()
        {
            while (true)
            {
                Console.Clear();
                Titulo("Gestión de Cursos");
                Console.WriteLine("1) Agregar");
                Console.WriteLine("2) Listar");
                Console.WriteLine("3) Asignar Profesor");
                Console.WriteLine("4) Volver");
                Console.WriteLine();

                var op = LeerOpcion("Opción", 1, 4);
                if (op == 4) return;

                try
                {
                    switch (op)
                    {
                        case 1:
                            var codigo = LeerNoVacio("Código del Curso");
                            var nombre = LeerNoVacio("Nombre del Curso");
                            var creditos = LeerEntero("Créditos (>0)", min: 1);
                            var profId = LeerNoVacio("ID Profesor Asignado");
                            var prof = RepoProfesores.BuscarPorId(profId) ?? throw new InvalidOperationException("Profesor no existe.");

                            var curso = new Curso(codigo, nombre, creditos, prof);
                            RepoCursos.Agregar(curso);
                            Exito("Curso agregado.");
                            Pausa();
                            break;

                        case 2:
                            ListarCursos();
                            Pausa();
                            break;

                        case 3:
                            var cod = LeerNoVacio("Código del Curso a reasignar");
                            var c = RepoCursos.BuscarPorId(cod);
                            if (c is null) { Aviso("Curso no existe."); Pausa(); break; }

                            var profNuevoId = LeerNoVacio("ID Profesor nuevo");
                            var profNuevo = RepoProfesores.BuscarPorId(profNuevoId);
                            if (profNuevo is null) { Aviso("Profesor no existe."); Pausa(); break; }

                            // Re-crear curso con nuevo profesor (si tu clase es inmutable)
                            var cursoNuevo = new Curso(c.Codigo, c.Nombre, c.Creditos, profNuevo);
                            RepoCursos.Eliminar(cod);
                            RepoCursos.Agregar(cursoNuevo);
                            Exito("Profesor asignado.");
                            Pausa();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    Pausa();
                }
            }
        }

        // ============================
        // MENÚ: MATRICULAR
        // ============================
        private static void MenuMatricular()
        {
            Console.Clear();
            Titulo("Matricular Estudiante en Curso");

            var idEst = LeerNoVacio("ID Estudiante");
            var est = RepoEstudiantes.BuscarPorId(idEst);
            if (est is null) { Aviso("Estudiante no existe."); Pausa(); return; }

            var cod = LeerNoVacio("Código Curso");
            var cur = RepoCursos.BuscarPorId(cod);
            if (cur is null) { Aviso("Curso no existe."); Pausa(); return; }

            Gestor.MatricularEstudiante(est, cur);
            Exito("Matrícula registrada.");
            Pausa();
        }

        // ============================
        // MENÚ: CALIFICACIONES
        // ============================
        private static void MenuCalificaciones()
        {
            Console.Clear();
            Titulo("Registrar Calificaciones");

            var idEst = LeerNoVacio("ID Estudiante");
            var cod = LeerNoVacio("Código Curso");
            var nota = LeerDecimal("Calificación (0-10)", 0m, 10m);

            Gestor.AgregarCalificacion(idEst, cod, nota);
            Exito("Calificación agregada.");
            Pausa();
        }

        // ============================
        // MENÚ: REPORTES
        // ============================
        private static void MenuReportes()
        {
            while (true)
            {
                Console.Clear();
                Titulo("Reportes / Estadísticas");
                Console.WriteLine("1) Reporte por Estudiante");
                Console.WriteLine("2) Top 10 Estudiantes");
                Console.WriteLine("3) Estudiantes en Riesgo (< 7.0)");
                Console.WriteLine("4) Cursos más Populares");
                Console.WriteLine("5) Promedio General");
                Console.WriteLine("6) Estadísticas por Carrera");
                Console.WriteLine("7) Volver");
                Console.WriteLine();

                var op = LeerOpcion("Opción", 1, 7);
                if (op == 7) return;

                Console.Clear();
                try
                {
                    switch (op)
                    {
                        case 1:
                            var id = LeerNoVacio("ID Estudiante");
                            Console.WriteLine(Gestor.GenerarReporteEstudiante(id));
                            Pausa();
                            break;

                        case 2:
                            var top = Gestor.ObtenerTop10Estudiantes();
                            if (top.Count == 0) { Aviso("Sin datos."); Pausa(); break; }
                            int rank = 1;
                            foreach (var e in top)
                                Console.WriteLine($"{rank++:00}. {e.Estudiante.Nombre} {e.Estudiante.Apellido} - {e.Promedio:0.00}");
                            Pausa();
                            break;

                        case 3:
                            var riesgo = Gestor.ObtenerEstudiantesEnRiesgo();
                            if (riesgo.Count == 0) { Aviso("Nadie en riesgo."); Pausa(); break; }
                            foreach (var e in riesgo)
                                Console.WriteLine($"{e.Identificacion} - {e.Nombre} {e.Apellido}");
                            Pausa();
                            break;

                        case 4:
                            var pop = Gestor.ObtenerCursosMasPopulares();
                            if (pop.Count == 0) { Aviso("Sin cursos."); Pausa(); break; }
                            foreach (var c in pop)
                                Console.WriteLine($"{c.Curso.Codigo} - {c.Curso.Nombre} | Estudiantes: {c.CantidadEstudiantes}");
                            Pausa();
                            break;

                        case 5:
                            Console.WriteLine($"Promedio General: {Gestor.ObtenerPromedioGeneral():0.00}");
                            Pausa();
                            break;

                        case 6:
                            var estats = Gestor.ObtenerEstadisticasPorCarrera();
                            if (estats.Count == 0) { Aviso("Sin datos."); Pausa(); break; }
                            foreach (var e in estats)
                                Console.WriteLine($"{e.Carrera} | Cant.: {e.Cantidad} | Prom.: {e.PromedioGeneral:0.00}");
                            Pausa();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    Pausa();
                }
            }
        }

        // ============================
        // MENÚ: REFLECTION
        // ============================
        private static void MenuReflection()
        {
            Console.Clear();
            Titulo("Análisis con Reflection");

            Console.WriteLine("Tipos disponibles:");
            Console.WriteLine("1) Estudiante");
            Console.WriteLine("2) Profesor");
            Console.WriteLine("3) Curso");
            Console.WriteLine();

            var op = LeerOpcion("Seleccione tipo", 1, 3);
            Type tipo = op switch
            {
                1 => typeof(Estudiante),
                2 => typeof(Profesor),
                _ => typeof(Curso)
            };

            Console.Clear();
            Titulo($"Reflection sobre {tipo.Name}");
            Console.WriteLine(AnalizadorReflection.MostrarPropiedades(tipo));
            Console.WriteLine(AnalizadorReflection.MostrarMetodos(tipo));
            Pausa();
        }

        // ============================
        // LISTADOS RÁPIDOS
        // ============================
        private static void ListarEstudiantes()
        {
            var todos = RepoEstudiantes.ObtenerTodos();
            if (todos.Count == 0) { Aviso("No hay estudiantes."); return; }
            foreach (var e in todos) Console.WriteLine(e);
        }

        private static void ListarProfesores()
        {
            var todos = RepoProfesores.ObtenerTodos();
            if (todos.Count == 0) { Aviso("No hay profesores."); return; }
            foreach (var p in todos) Console.WriteLine(p);
        }

        private static void ListarCursos()
        {
            var todos = RepoCursos.ObtenerTodos();
            if (todos.Count == 0) { Aviso("No hay cursos."); return; }
            foreach (var c in todos) Console.WriteLine(c);
        }

        // ============================
        // CREACIÓN INTERACTIVA
        // ============================
        private static Estudiante CrearEstudianteInteractivo()
        {
            var id = LeerNoVacio("ID Estudiante");
            var nom = LeerNoVacio("Nombre");
            var ape = LeerNoVacio("Apellido");
            var nac = LeerFecha("Fecha Nacimiento (yyyy-MM-dd)");
            var carr = LeerNoVacio("Carrera");
            var matr = LeerNoVacio("N° Matrícula (ej. ABC-12345)");

            return new Estudiante(id, nom, ape, nac, carr, matr);
        }

        private static Profesor CrearProfesorInteractivo()
        {
            var id = LeerNoVacio("ID Profesor");
            var nom = LeerNoVacio("Nombre");
            var ape = LeerNoVacio("Apellido");
            var nac = LeerFecha("Fecha Nacimiento (yyyy-MM-dd)");
            var dpto = LeerNoVacio("Departamento");
            var tipo = LeerEnum<TipoContrato>("Tipo Contrato [TiempoCompleto, MedioTiempo, Temporal, Honorarios]");
            var sal = LeerDecimal("Salario Base (>=0)", 0m, decimal.MaxValue);

            return new Profesor(id, nom, ape, nac, dpto, tipo, sal);
        }

        // ============================
        // HELPERS DE ENTRADA / UI
        // ============================
        private static int LeerOpcion(string etiqueta, int min, int max)
        {
            while (true)
            {
                Console.Write($"{etiqueta} [{min}-{max}]: ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out var v) && v >= min && v <= max) return v;
                Error("Opción inválida.");
            }
        }

        private static string LeerNoVacio(string etiqueta)
        {
            while (true)
            {
                Console.Write($"{etiqueta}: ");
                var s = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Error("Valor requerido.");
            }
        }

        private static string LeerOpcional(string etiqueta)
        {
            Console.Write($"{etiqueta}: ");
            return Console.ReadLine() ?? "";
        }

        private static int LeerEntero(string etiqueta, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write($"{etiqueta}: ");
                var s = Console.ReadLine();
                if (int.TryParse(s, out var v) && v >= min && v <= max) return v;
                Error("Número inválido.");
            }
        }

        private static decimal LeerDecimal(string etiqueta, decimal min, decimal max)
        {
            while (true)
            {
                Console.Write($"{etiqueta}: ");
                var s = Console.ReadLine();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v >= min && v <= max)
                    return v;
                Error($"Debe estar entre {min} y {max}.");
            }
        }

        private static decimal? LeerDecimalOpcional(string etiqueta)
        {
            Console.Write($"{etiqueta}: ");
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v)) return v;
            Error("Número inválido. Se ignora.");
            return null;
        }

        private static DateTime LeerFecha(string etiqueta)
        {
            while (true)
            {
                Console.Write($"{etiqueta}: ");
                var s = Console.ReadLine();
                if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return dt;
                Error("Formato inválido. Use yyyy-MM-dd.");
            }
        }

        private static TEnum LeerEnum<TEnum>(string etiqueta) where TEnum : struct, Enum
        {
            while (true)
            {
                Console.Write($"{etiqueta}: ");
                var s = Console.ReadLine();
                if (Enum.TryParse<TEnum>(s, true, out var v)) return v;
                Error("Valor inválido.");
            }
        }

        private static void Titulo(string texto)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(texto);
            Console.WriteLine(new string('=', texto.Length));
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void Exito(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void Aviso(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void Informar(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void Pausa()
        {
            Console.WriteLine();
            Console.Write("Presione ENTER para continuar...");
            Console.ReadLine();
        }

        // ============================
        // DATOS DE PRUEBA
        // ============================
        private static void SembrarDatosIniciales()
        {
            try
            {
                var p1 = new Profesor("PRO-1", "Laura", "Suárez", new DateTime(1982, 5, 14), "Computación", TipoContrato.TiempoCompleto, 80000m);
                var p2 = new Profesor("PRO-2", "Carlos", "Ibarra", new DateTime(1975, 3, 22), "Matemática", TipoContrato.MedioTiempo, 50000m);
                RepoProfesores.Agregar(p1);
                RepoProfesores.Agregar(p2);

                var c1 = new Curso("CS101", "Programación I", 4, p1);
                var c2 = new Curso("CS102", "Estructuras de Datos", 4, p1);
                var c3 = new Curso("MA101", "Cálculo I", 5, p2);
                RepoCursos.Agregar(c1);
                RepoCursos.Agregar(c2);
                RepoCursos.Agregar(c3);

                var e1 = new Estudiante("EST-1", "Ana", "Mora", new DateTime(2002, 7, 10), "Ing. Sistemas", "ABC-12345");
                var e2 = new Estudiante("EST-2", "Luis", "Vega", new DateTime(2001, 1, 20), "Ing. Sistemas", "DEF-67890");
                var e3 = new Estudiante("EST-3", "María", "Cruz", new DateTime(2000, 11, 5), "Matemática", "GHI-24680");
                RepoEstudiantes.Agregar(e1);
                RepoEstudiantes.Agregar(e2);
                RepoEstudiantes.Agregar(e3);

                // Matrículas y notas de ejemplo
                Gestor.MatricularEstudiante(e1, c1);
                Gestor.MatricularEstudiante(e1, c2);
                Gestor.MatricularEstudiante(e2, c1);
                Gestor.MatricularEstudiante(e3, c3);

                Gestor.AgregarCalificacion("EST-1", "CS101", 9.0m);
                Gestor.AgregarCalificacion("EST-1", "CS101", 8.5m);
                Gestor.AgregarCalificacion("EST-1", "CS102", 7.2m);

                Gestor.AgregarCalificacion("EST-2", "CS101", 6.8m);
                Gestor.AgregarCalificacion("EST-2", "CS101", 7.1m);

                Gestor.AgregarCalificacion("EST-3", "MA101", 5.9m);
                Gestor.AgregarCalificacion("EST-3", "MA101", 6.4m);
            }
            catch
            {
                // Ignorar duplicados si se ejecuta más de una vez
            }
        }

        private const string DATA_FILE = "datos.json";

        // Crea el snapshot con TODO el estado actual
        private static AppState CrearSnapshot()
        {
            // Estudiantes y profesores tal cual
            var ests = RepoEstudiantes.ObtenerTodos().ToList();
            var profs = RepoProfesores.ObtenerTodos().ToList();

            // Cursos → DTO con ProfesorId (ya corregiste a ProfesorAsignado)
            var cursosDto = RepoCursos.ObtenerTodos()
                .Select(c => new CursoDTO(c.Codigo, c.Nombre, c.Creditos, c.ProfesorAsignado.Identificacion))
                .ToList();

            // Matrículas → DTO con IDs + calificaciones
            var matsDto = Gestor.ExportarMatriculas()
                .Select(m => new MatriculaDTO(
                    m.Estudiante.Identificacion,
                    m.Curso.Codigo,
                    m.FechaMatricula,          // ← antes usabas m.Fecha
                    m.Calificaciones.ToList()  // ← antes llamabas ObtenerCalificaciones()
                ))
                .ToList();


            return new AppState(ests, profs, cursosDto, matsDto);
        }

        // Reconstruye el estado desde un snapshot
        private static void RestaurarDesdeSnapshot(AppState state)
        {
            // Limpia y repuebla repos
            foreach (var e in state.Estudiantes) RepoEstudiantes.Agregar(e);
            foreach (var p in state.Profesores) RepoProfesores.Agregar(p);

            // Cursos: vincula profesor por id
            foreach (var c in state.Cursos)
            {
                var prof = RepoProfesores.BuscarPorId(c.ProfesorId)
                           ?? throw new InvalidOperationException($"Profesor {c.ProfesorId} no existe (snapshot).");
                RepoCursos.Agregar(new Curso(c.Codigo, c.Nombre, c.Creditos, prof));
            }

            // Matrículas: vincula estudiante/curso y repone notas
            foreach (var m in state.Matriculas)
            {
                var est = RepoEstudiantes.BuscarPorId(m.EstudianteId)
                          ?? throw new InvalidOperationException($"Estudiante {m.EstudianteId} no existe (snapshot).");
                var cur = RepoCursos.BuscarPorId(m.CursoCodigo)
                          ?? throw new InvalidOperationException($"Curso {m.CursoCodigo} no existe (snapshot).");

                Gestor.MatricularEstudiante(est, cur);

                foreach (var nota in m.Calificaciones)
                    Gestor.AgregarCalificacion(m.EstudianteId, m.CursoCodigo, nota);
            }
        }

        private static void GuardarTodo()
        {
            var state = CrearSnapshot();
            JsonHelper.GuardarObjeto(state, DATA_FILE);
        }

        private static void CargarTodo()
        {
            var state = JsonHelper.CargarObjeto<AppState>(DATA_FILE);
            if (state is not null)
                RestaurarDesdeSnapshot(state);
        }

    }
}
