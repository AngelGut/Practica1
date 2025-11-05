using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Practica1.Interfeces;

namespace Practica1.Clases
{
    abstract internal class Persona : IIdentificable
    {
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaNacimiento { get; set; }

        //Propiedad calculada para obtener la edad
        public int Edad
        {
            get
            {
                //obtenemos el dia de hoy
                var hoy = DateTime.Today;

                //calculamos la edad
                int edad = hoy.Year - FechaNacimiento.Year;

                //si no ha cumplido años este año, le restamos 1
                if (hoy < FechaNacimiento.AddYears(edad)) edad--;
                return edad;
            }
        }
        //Método abstracto para obtener el rol
        abstract public string ObtenerRol();

        //Constructor protegido esto significa que solo las clases derivadas pueden instanciarlo
        //Nadie desde fuera puede hacer new PersonaAbs(...) porque es abstracta.
        //Además, las validaciones básicas se hacen aquí.
        protected Persona(string identificacion, string nombre, string apellido, DateTime fechaNacimiento)
        {
            // Validaciones básicas
            //Identificacion, nombre y apellido no pueden ser nulos o vacíos
            if (string.IsNullOrWhiteSpace(identificacion)) throw new ArgumentException("La identificación es obligatoria.", nameof(identificacion));

            //Nombre y apellido no pueden ser nulos o vacíos
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));

            //Apellido no puede ser nulo o vacío
            if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));

            //Fecha de nacimiento no puede ser futura
            if (fechaNacimiento > DateTime.Today) throw new ArgumentOutOfRangeException(nameof(fechaNacimiento), "La fecha de nacimiento no puede ser futura.");

            Identificacion = identificacion.Trim();
            Nombre = nombre.Trim();
            Apellido = apellido.Trim();
            FechaNacimiento = fechaNacimiento.Date;
        }

        // Método de ayuda para validaciones de edad en derivadas
        protected void ValidarEdadMinima(int edadMinima)
        {
            if (Edad < edadMinima)
                throw new InvalidOperationException($"La edad mínima es {edadMinima} años. Edad actual: {Edad}.");
        }


        // Requisito 4: ToString formateado
        public override string ToString()
        {
            return $"{ObtenerRol()}: {Nombre} {Apellido} | ID: {Identificacion} | Nacimiento: {FechaNacimiento:yyyy-MM-dd} | Edad: {Edad}";
        }
    }
}
