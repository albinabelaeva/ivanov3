using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Todo.Core
{
    public class TodoList
    {
        private readonly List<TodoItem> _items = new();
        public IReadOnlyList<TodoItem> Items => _items.AsReadOnly();
        public TodoItem Add(string title)
        {
            var item = new TodoItem(title);
            _items.Add(item);
            return item;
        }
        public bool Remove(Guid id) => _items.RemoveAll(i => i.Id == id) > 0;
        public IEnumerable<TodoItem> Find(string substring) =>
        _items.Where(i => i.Title.Contains(substring ?? string.Empty,
       StringComparison.OrdinalIgnoreCase));
        public int Count => _items.Count;

        // Метод для сохранения в JSON
        public void Save(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(_items, options);
            File.WriteAllText(filePath, json);
        }

        // Метод для загрузки из JSON
        public static TodoList Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<TodoItem>>(json, options);

            var todoList = new TodoList();
            if (items != null)
            {
                // Используем рефлексию для добавления items в приватный список
                // Или добавляем через AddRange через рефлексию
                var field = typeof(TodoList).GetField("_items",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(todoList, items);
                }
            }

            return todoList;
        }

        // Альтернативный вариант: метод для очистки и загрузки
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = JsonSerializer.Deserialize<List<TodoItem>>(json, options);

            _items.Clear();
            if (items != null)
            {
                _items.AddRange(items);
            }
        }
    }
}
