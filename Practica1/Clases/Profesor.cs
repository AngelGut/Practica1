using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Clases
{
    // Enum requerido para Profesor
    enum TipoContrato
    {
        TiempoCompleto,
        MedioTiempo,
        Temporal,
        Honorarios
    }

    internal class Profesor : Persona
    {
        public string Departamento { get; }
        public TipoContrato TipoContrato { get; }
        public decimal SalarioBase { get; }

        public Profesor(
            string identificacion,
            string nombre,
            string apellido,
            DateTime fechaNacimiento,
            string departamento,
            TipoContrato tipoContrato,
            decimal salarioBase)
            : base(identificacion, nombre, apellido, fechaNacimiento)
        {
            if (string.IsNullOrWhiteSpace(departamento)) throw new ArgumentException("El departamento es obligatorio.", nameof(departamento));
            if (salarioBase < 0m) throw new ArgumentOutOfRangeException(nameof(salarioBase), "El salario base no puede ser negativo.");

            Departamento = departamento.Trim();
            TipoContrato = tipoContrato;
            SalarioBase = salarioBase;

            // Requisito 8: Validación de edad mínima
            ValidarEdadMinima(25);
        }

        public override string ObtenerRol() => "Profesor";

        public override string ToString() =>
            base.ToString() + $" | Depto.: {Departamento} | Contrato: {TipoContrato} | Salario: {SalarioBase:C}";
    }
}
