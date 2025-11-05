using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Atributos
{
    /// <summary>
    /// Clase Validador
    /// Se encarga de inspeccionar un objeto con Reflection,
    /// buscar atributos personalizados como [ValidacionRango]
    /// y verificar si los valores cumplen las reglas definidas.
    /// 
    /// Retorna una lista de errores si alguna validación falla.
    /// </summary>
    internal static class Validador
    {
        /// <summary>
        /// Valida una instancia de cualquier clase que tenga atributos personalizados.
        /// </summary>
        /// <param name="instancia">El objeto a validar.</param>
        /// <returns>Una lista de mensajes de error. Si está vacía, el objeto es válido.</returns>
        public static List<string> Validar(object instancia)
        {
            // Lista donde se guardarán los errores encontrados
            var errores = new List<string>();

            // Si el objeto recibido es nulo, no hay nada que validar
            if (instancia == null)
                throw new ArgumentNullException(nameof(instancia), "La instancia no puede ser nula.");

            // Obtenemos el tipo del objeto (por ejemplo: typeof(Profesor))
            Type tipo = instancia.GetType();

            // Recorremos todas las propiedades públicas de esa clase
            foreach (var propiedad in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Obtenemos el valor actual de la propiedad
                var valor = propiedad.GetValue(instancia);

                // ---------------------------------------------------------------------
                // Validación de Rango → busca el atributo [ValidacionRango(...)]
                // ---------------------------------------------------------------------
                var atributoRango = propiedad.GetCustomAttribute<ValidacionRangoAttribute>();
                if (atributoRango != null)
                {
                    // Si el valor no es nulo y puede convertirse a número, lo validamos
                    if (valor is IConvertible)
                    {
                        try
                        {
                            decimal valorDecimal = Convert.ToDecimal(valor);

                            if (valorDecimal < atributoRango.Minimo || valorDecimal > atributoRango.Maximo)
                            {
                                errores.Add(
                                    $"Error en '{propiedad.Name}': valor {valorDecimal} fuera del rango " +
                                    $"({atributoRango.Minimo} - {atributoRango.Maximo})."
                                );
                            }
                        }
                        catch
                        {
                            errores.Add($"Error en '{propiedad.Name}': el valor no es numérico, no puede validarse el rango.");
                        }
                    }
                }
            }

            // Devolvemos la lista de errores (vacía si todo está bien)
            return errores;
        }
    }
}
