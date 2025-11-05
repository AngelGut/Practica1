using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Interfeces
{
    //Interfaz para evaluar calificaciones
    public interface IEvaluable
    {
        void AgregarCalificacion(decimal calificacion);
        decimal ObtenerPromedio();
        bool HaAprobado();
        string ObtenerEstado(); // "Aprobado", "Reprobado" o "En Curso"
    }
}
