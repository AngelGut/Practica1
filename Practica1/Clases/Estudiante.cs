using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Clases
{
    internal class Estudiante : Persona
    {
        // Propiedades específicas de Estudiante
        public string Carrera { get; }
        public string NumeroMatricula { get; }

        // Constructor
        public Estudiante(
            string identificacion,
            string nombre,
            string apellido,
            DateTime fechaNacimiento,
            string carrera,
            string numeroMatricula)
            : base(identificacion, nombre, apellido, fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(carrera)) throw new ArgumentException("La carrera es obligatoria.", nameof(carrera));
            if (string.IsNullOrWhiteSpace(numeroMatricula)) throw new ArgumentException("El número de matrícula es obligatorio.", nameof(numeroMatricula));

            Carrera = carrera.Trim();
            NumeroMatricula = numeroMatricula.Trim();

            // Requisito 8: Validación de edad mínima
            ValidarEdadMinima(15);
        }

        // Implementación del método abstracto ObtenerRol
        public override string ObtenerRol() => "Estudiante";

        //ToString formateado
        public override string ToString() =>
            base.ToString() + $" | Carrera: {Carrera} | Matrícula: {NumeroMatricula}";
    }
}
