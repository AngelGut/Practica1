using System.Text;

namespace Practica1.Servicios
{
    internal static class TablaConsola
    {
        // Dibuja una tabla con colores (borders, header, zebra)
        public static void EscribirTabla(
            IReadOnlyList<string> encabezados,
            IEnumerable<IReadOnlyList<string>> filas,
            ConsoleColor colorBorde = ConsoleColor.DarkGray,
            ConsoleColor colorHeader = ConsoleColor.Yellow,
            ConsoleColor colorFila = ConsoleColor.White,
            ConsoleColor colorFilaAlterna = ConsoleColor.Gray)
        {
            if (encabezados == null || encabezados.Count == 0) return;

            // Materializamos filas para calcular anchos y poder iterar dos veces
            var listaFilas = filas?.Select(f => f?.Select(c => c ?? string.Empty).ToList() ?? new List<string>())
                                   .ToList() ?? new List<List<string>>();

            int columnas = encabezados.Count;
            var anchos = new int[columnas];

            // Anchos por columna
            for (int i = 0; i < columnas; i++)
                anchos[i] = encabezados[i].Length;

            foreach (var fila in listaFilas)
                for (int i = 0; i < columnas && i < fila.Count; i++)
                    anchos[i] = Math.Max(anchos[i], fila[i].Length);

            string Separador()
                => "+" + string.Join("+", anchos.Select(a => new string('-', a + 2))) + "+";

            void EscribirBorde()
            {
                var sep = Separador();
                Console.ForegroundColor = colorBorde;
                Console.WriteLine(sep);
                Console.ResetColor();
            }

            // Header
            EscribirBorde();
            Console.ForegroundColor = colorHeader;
            Console.WriteLine("| " + string.Join(" | ", encabezados.Select((h, i) => h.PadRight(anchos[i]))) + " |");
            Console.ResetColor();
            EscribirBorde();

            // Filas (zebra)
            int idx = 0;
            foreach (var fila in listaFilas)
            {
                Console.ForegroundColor = (idx++ % 2 == 0) ? colorFila : colorFilaAlterna;
                var celdas = Enumerable.Range(0, columnas)
                                       .Select(i => (i < fila.Count ? fila[i] : "").PadRight(anchos[i]));
                Console.WriteLine("| " + string.Join(" | ", celdas) + " |");
                Console.ResetColor();
            }

            EscribirBorde();
        }
    }
}
