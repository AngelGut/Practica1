using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Modelos
{
    /// <summary>
    ///   - Todos los métodos son estáticos para que no necesites instanciar la clase.
    ///   - Las operaciones devuelven strings cuando corresponde; el llamador decide si imprimirlos en consola.
    /// </summary>
    internal class AnalizadorReflection
    {
        // ----------------------------------------------------------------------------------------
        // Método: MostrarPropiedades
        // Devuelve un listado con todas las propiedades públicas de "tipo", incluyendo su tipo.
        // ----------------------------------------------------------------------------------------
        public static string MostrarPropiedades(Type tipo)
        {
            // Validación defensiva: no aceptar un Type nulo.
            if (tipo is null) throw new ArgumentNullException(nameof(tipo));

            // StringBuilder es eficiente para construir texto de varias líneas.
            var sb = new StringBuilder();

            // Encabezado informativo.
            sb.AppendLine($"Propiedades de {tipo.FullName}:");
            sb.AppendLine(new string('-', 60));

            // Obtiene propiedades públicas (instancia y estáticas) declaradas en el tipo o heredadas.
            var propiedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            // Si no hay propiedades, lo indicamos explícitamente.
            if (propiedades.Length == 0)
            {
                sb.AppendLine("(sin propiedades públicas)");
                return sb.ToString();
            }

            // Recorremos las propiedades y mostramos "TipoPropiedad NombrePropiedad { get; set; }"
            foreach (var p in propiedades)
            {
                // Determina qué accesores tiene (get y/o set).
                var tieneGet = p.GetGetMethod(nonPublic: false) != null;
                var tieneSet = p.GetSetMethod(nonPublic: false) != null;
                var accesores = (tieneGet, tieneSet) switch
                {
                    (true, true) => "{ get; set; }",
                    (true, false) => "{ get; }",
                    (false, true) => "{ set; }",
                    _ => "{ }"
                };

                // Tipo legible de la propiedad.
                var tipoProp = FormatearNombreTipo(p.PropertyType);

                sb.AppendLine($"{tipoProp} {p.Name} {accesores}");
            }

            return sb.ToString();
        }

        // ----------------------------------------------------------------------------------------
        // Método: MostrarMetodos
        // Devuelve un listado con todos los métodos públicos del tipo, mostrando su firma.
        // Excluye los métodos heredados de System.Object para que el resultado sea más útil.
        // ----------------------------------------------------------------------------------------
        public static string MostrarMetodos(Type tipo)
        {
            if (tipo is null) throw new ArgumentNullException(nameof(tipo));

            var sb = new StringBuilder();
            sb.AppendLine($"Métodos públicos de {tipo.FullName}:");
            sb.AppendLine(new string('-', 60));

            // Obtiene métodos públicos de instancia y estáticos.
            var metodos = tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                              // Filtramos los métodos de Object (ToString, GetHashCode, Equals, GetType) para claridad.
                              .Where(m => m.DeclaringType != typeof(object))
                              .OrderBy(m => m.Name)
                              .ToArray();

            if (metodos.Length == 0)
            {
                sb.AppendLine("(sin métodos públicos)");
                return sb.ToString();
            }

            foreach (var m in metodos)
            {
                // Tipo de retorno legible.
                var retorno = FormatearNombreTipo(m.ReturnType);

                // Parametrización: "tipo nombre" por cada parámetro.
                var parametros = m.GetParameters();
                var firmaParametros = string.Join(", ",
                    parametros.Select(p => $"{FormatearNombreTipo(p.ParameterType)} {p.Name}"));

                // Determinar si es estático para informarlo.
                var prefijo = m.IsStatic ? "static " : "";

                sb.AppendLine($"{prefijo}{retorno} {m.Name}({firmaParametros})");
            }

            return sb.ToString();
        }

        // ----------------------------------------------------------------------------------------
        // Método: CrearInstanciaDinamica
        // Crea una instancia del tipo indicado usando el constructor que mejor coincida con los parámetros.
        // Usa Activator.CreateInstance para resolver el constructor adecuado.
        // ----------------------------------------------------------------------------------------
        public static object CrearInstanciaDinamica(Type tipo, params object[] parametros)
        {
            if (tipo is null) throw new ArgumentNullException(nameof(tipo));

            try
            {
                // Activator.CreateInstance intentará localizar el constructor que mejor encaje con los argumentos.
                // Si "parametros" está vacío, buscará un constructor sin parámetros.
                var instancia = Activator.CreateInstance(tipo, parametros);
                if (instancia is null)
                {
                    // Activator podría devolver null en casos muy atípicos; lo tratamos como error explícito.
                    throw new InvalidOperationException($"No fue posible crear una instancia de {tipo.FullName}.");
                }
                return instancia;
            }
            catch (MissingMethodException)
            {
                // Cuando no existe un constructor que calce con los parámetros dados.
                var firmas = string.Join(", ", parametros.Select(p => p?.GetType().Name ?? "null"));
                throw new InvalidOperationException(
                    $"No se encontró un constructor en {tipo.FullName} compatible con los parámetros: ({firmas}).");
            }
            catch (TargetInvocationException ex)
            {
                // Si el constructor lanzó una excepción, la superficie con contexto claro.
                throw new InvalidOperationException(
                    $"El constructor de {tipo.FullName} lanzó una excepción: {ex.InnerException?.Message}", ex.InnerException);
            }
        }

        // ----------------------------------------------------------------------------------------
        // Método: InvocarMetodo
        // Invoca un método por nombre sobre una instancia, eligiendo la sobrecarga que mejor
        // se ajuste a los parámetros proporcionados. Devuelve el valor de retorno (o null si es void).
        // ----------------------------------------------------------------------------------------
        public static object? InvocarMetodo(object instancia, string nombreMetodo, params object[] parametros)
        {
            if (instancia is null) throw new ArgumentNullException(nameof(instancia));
            if (string.IsNullOrWhiteSpace(nombreMetodo)) throw new ArgumentException("Nombre de método obligatorio.", nameof(nombreMetodo));

            var tipo = instancia.GetType();

            // Intento 1: búsqueda directa por firma exacta de tipos de parámetros.
            var tiposParametros = parametros?.Select(p => p?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;

            // GetMethod con tipos exactos puede fallar si hay conversión implícita requerida o parámetros null.
            var metodo = tipo.GetMethod(nombreMetodo, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                                        binder: null, types: tiposParametros, modifiers: null);

            if (metodo is null)
            {
                // Intento 2: elegir manualmente la primera sobrecarga compatible por:
                //   - mismo nombre
                //   - misma cantidad de parámetros
                //   - cada argumento "encaja" en el tipo del parámetro (IsInstanceOfType o asignable)
                metodo = tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                             .Where(m => m.Name == nombreMetodo)
                             .FirstOrDefault(m =>
                             {
                                 var ps = m.GetParameters();
                                 if (ps.Length != tiposParametros.Length) return false;

                                 for (int i = 0; i < ps.Length; i++)
                                 {
                                     var pTipo = ps[i].ParameterType;
                                     var arg = parametros[i];

                                     // Si el argumento es null, solo encaja si el parámetro es referencia o Nullable<>
                                     if (arg is null)
                                     {
                                         if (pTipo.IsValueType && Nullable.GetUnderlyingType(pTipo) is null)
                                             return false;
                                         continue;
                                     }

                                     // Si el tipo del argumento es asignable al tipo del parámetro, es compatible.
                                     if (!pTipo.IsAssignableFrom(arg.GetType()))
                                         return false;
                                 }
                                 return true;
                             });
            }

            if (metodo is null)
            {
                var firmas = string.Join(", ", tiposParametros.Select(t => t.Name));
                throw new MissingMethodException(
                    $"No se encontró un método público '{nombreMetodo}({firmas})' en {tipo.FullName}.");
            }

            try
            {
                // Invoca el método encontrado. Para métodos estáticos, 'instancia' puede ignorarse.
                var resultado = metodo.Invoke(metodo.IsStatic ? null : instancia, parametros);

                // Si el método es void, Invoke devuelve null.
                return resultado;
            }
            catch (TargetInvocationException ex)
            {
                // Si el método invocado lanza una excepción, exponemos el mensaje interno.
                throw new InvalidOperationException(
                    $"La invocación de {tipo.FullName}.{nombreMetodo} lanzó una excepción: {ex.InnerException?.Message}",
                    ex.InnerException);
            }
        }

        // ----------------------------------------------------------------------------------------
        // Utilidad: FormatearNombreTipo
        // Devuelve un nombre legible de un Type (por ejemplo, para genéricos List<int>).
        // ----------------------------------------------------------------------------------------
        private static string FormatearNombreTipo(Type t)
        {
            // Tipos nulos por referencia en C# 8+ podrían llegar como Nullable<T>; los formateamos como "Tipo?".
            var subyacenteNullable = Nullable.GetUnderlyingType(t);
            if (subyacenteNullable != null)
                return $"{FormatearNombreTipo(subyacenteNullable)}?";

            // Para tipos genéricos (List<T>, Dictionary<K,V>), mostramos "NombreGenérico<Args>"
            if (t.IsGenericType)
            {
                var nombre = t.Name;
                var tickIndex = nombre.IndexOf('`'); // Elimina la parte `1, `2, etc. de tipos genéricos.
                if (tickIndex >= 0) nombre = nombre[..tickIndex];

                var args = t.GetGenericArguments().Select(FormatearNombreTipo);
                return $"{nombre}<{string.Join(", ", args)}>";
            }

            // Nombres simples para tipos comunes (Int32 -> int, String -> string) para salida más amigable.
            return t == typeof(int) ? "int" :
                   t == typeof(string) ? "string" :
                   t == typeof(bool) ? "bool" :
                   t == typeof(void) ? "void" :
                   t == typeof(decimal) ? "decimal" :
                   t == typeof(double) ? "double" :
                   t == typeof(float) ? "float" :
                   t.Name;
        }
    }
}