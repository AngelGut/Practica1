using Practica1.Interfeces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Practica1.Clases;

namespace Practica1.Modelos
{
    internal class Matricula : IEvaluable
    {

        // Constante de nota mínima para aprobar
        public const decimal NotaMinimaAprobacion = 7.0m;

        // Propiedades
        public Estudiante Estudiante { get; }
        public Curso Curso { get; }
        public DateTime FechaMatricula { get; }

        //declaramos la lista de calificaciones
        public List<decimal> Calificaciones { get; } = new();

        // Constructor
        public Matricula(Estudiante estudiante, Curso curso, DateTime fechaMatricula)
        {
            Estudiante = estudiante ?? throw new ArgumentNullException(nameof(estudiante));
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            FechaMatricula = fechaMatricula;
        }

        // IEvaluable aqui implementa los metodos de la interfaz
        // Agrega una calificación a la lista
        public void AgregarCalificacion(decimal calificacion)
        {
            if (calificacion < 0m || calificacion > 10m)
                throw new ArgumentOutOfRangeException(nameof(calificacion), "La calificación debe estar entre 0 y 10.");

            Calificaciones.Add(decimal.Round(calificacion, 2)); // normaliza a 2 decimales
        }

        // Calcula el promedio de las calificaciones
        public decimal ObtenerPromedio()
        {
            if (Calificaciones.Count == 0) return 0m;
            return Math.Round(Calificaciones.Average(), 2, MidpointRounding.AwayFromZero); //el average es una manera de promediar internamente en la lista muy util para espacio
            //lo del average tambien es LINQ (es una tecnología de consultas integrada en C#)
            //MidpointRounding.AwayFromZero es un enum que define cómo redondear cuando el número termina exactamente en 0.5.
        }

        // Determina si el estudiante ha aprobado (en expresion lambda)
        public bool HaAprobado() => Calificaciones.Count > 0 && ObtenerPromedio() >= NotaMinimaAprobacion;

        //aqui planteamos si esta aprobado, reprobado o en curso
        public string ObtenerEstado()
        {
            if (Calificaciones.Count == 0) return "En Curso";
            return HaAprobado() ? "Aprobado" : "Reprobado";
        }

        public override string ToString()
        {
            var prom = ObtenerPromedio();
            return $"{Estudiante?.Nombre} {Estudiante?.Apellido} | {Curso?.Codigo}-{Curso?.Nombre} | " +
                   $"Promedio: {prom:0.00} | Estado: {ObtenerEstado()}";
        }
    }
}

