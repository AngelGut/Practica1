using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Atributos
{/// <summary>
 /// Atributo personalizado que permite definir un rango numérico permitido
 /// para una propiedad (por ejemplo: entre 0 y 100).
 /// 
 /// Se usa de la siguiente manera:
 /// 
 ///     [ValidacionRango(500, 10000)]
 ///     public decimal SalarioBase { get; set; }
 /// 
 /// Luego, la clase "Validador" usará Reflection para leer este atributo
 /// y verificar si el valor real de la propiedad está dentro del rango permitido.
 /// </summary>

    // AttributeUsage indica dónde se puede aplicar este atributo.
    // En este caso, solo sobre PROPIEDADES.
    [AttributeUsage(AttributeTargets.Property)]
    internal class ValidacionRangoAttribute : Attribute
    {
        // ----------------------------------------------------------
        // Propiedades del atributo
        // ----------------------------------------------------------

        // Valor mínimo permitido para la propiedad decorada.
        public decimal Minimo { get; }

        // Valor máximo permitido para la propiedad decorada.
        public decimal Maximo { get; }

        // ----------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------

        /// <summary>
        /// Crea una nueva instancia del atributo con los límites especificados.
        /// 
        /// Ejemplo:
        ///     [ValidacionRango(0, 100)]
        ///     public int Edad { get; set; }
        /// 
        /// Esto indica que la propiedad "Edad" debe estar entre 0 y 100.
        /// </summary>
        /// <param name="min">Valor mínimo permitido.</param>
        /// <param name="max">Valor máximo permitido.</param>
        public ValidacionRangoAttribute(decimal min, decimal max)
        {
            // Asignamos los valores pasados al constructor
            Minimo = min;
            Maximo = max;
        }
    }
}
