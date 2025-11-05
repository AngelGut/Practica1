using Practica1.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Practica1.Interfeces;

namespace Practica1.Modelos
{
    internal class Curso : IIdentificable
    {
        //Propiedades del curso
        public string Codigo { get; }
        public string Nombre { get; }
        public int Creditos { get; }
        public string Identificacion => Codigo;

        public Profesor ProfesorAsignado { get; }

        //Constructor del curso
        public Curso(string codigo, string nombre, int creditos, Profesor profesorAsignado)
        {
            if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Código obligatorio.", nameof(codigo));
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre obligatorio.", nameof(nombre));
            if (creditos <= 0) throw new ArgumentOutOfRangeException(nameof(creditos), "Créditos debe ser > 0.");
            ProfesorAsignado = profesorAsignado ?? throw new ArgumentNullException(nameof(profesorAsignado));

            Codigo = codigo.Trim();
            Nombre = nombre.Trim();
            Creditos = creditos;
        }

        //ToString formateado
        public override string ToString() => $"{Codigo} - {Nombre} ({Creditos} cr.) | Prof.: {ProfesorAsignado?.Nombre} {ProfesorAsignado?.Apellido}";
    }
}

