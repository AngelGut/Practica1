using Practica1.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practica1.Servicios
{

    internal record CursoDTO(string Codigo, string Nombre, int Creditos, string ProfesorId);
    internal record MatriculaDTO(string EstudianteId, string CursoCodigo, DateTime Fecha, List<decimal> Calificaciones);
    internal record AppState(
        List<Estudiante> Estudiantes,
        List<Profesor> Profesores,
        List<CursoDTO> Cursos,
        List<MatriculaDTO> Matriculas
    );

}
