using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Practica1.Servicios
{
    internal static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };

        public static void GuardarEnJson<T>(IEnumerable<T> datos, string rutaArchivo)
        {
            var json = JsonSerializer.Serialize(datos, _options);
            File.WriteAllText(rutaArchivo, json);
        }

        public static List<T> CargarDesdeJson<T>(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo)) return new List<T>();
            var json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }

        public static void GuardarObjeto<T>(T data, string rutaArchivo)
        {
            var json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(rutaArchivo, json);
        }

        public static T? CargarObjeto<T>(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo)) return default;
            var json = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}
