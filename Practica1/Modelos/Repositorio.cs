using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Practica1.Interfeces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Practica1.Modelos
{
    internal class Repositorio<T> where T : IIdentificable
    {
        // Comparador para ignorar mayúsculas/minúsculas en IDs
        private readonly Dictionary<string, T> _items = new(StringComparer.OrdinalIgnoreCase);

        // Métodos CRUD básicos
        public void Agregar(T item)
        {
            // Validacion de que no sea nulo y que la identificación sea única
            if (item is null) throw new ArgumentNullException(nameof(item));

            // Validar que la identificación no sea nula o vacía
            if (string.IsNullOrWhiteSpace(item.Identificacion))
                throw new ArgumentException("La identificación es obligatoria.", nameof(item));

            // Validar unicidad de identificación
            if (_items.ContainsKey(item.Identificacion))
                throw new InvalidOperationException("Ya existe un elemento con esa identificación.");

            // Agregar al diccionario (Un diccionario (Dictionary<TKey, TValue>) es una colección genérica de pares) clave -> valor.
            _items.Add(item.Identificacion, item);
            //Usa la identificación como clave.
            //Usa el objeto completo como valor.
        }

        public bool Eliminar(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return _items.Remove(id);
        }

        public T? BuscarPorId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return default;
            _items.TryGetValue(id, out var item);
            return item;
        }

        public IReadOnlyCollection<T> ObtenerTodos()
            => new ReadOnlyCollection<T>(_items.Values.ToList());

        // Búsqueda por predicado (delegates / lambdas)
        public IEnumerable<T> Buscar(Func<T, bool> predicado)
        {
            if (predicado is null) throw new ArgumentNullException(nameof(predicado));
            return _items.Values.Where(predicado);
        }
    }
}

