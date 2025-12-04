using Xunit;
using Todo.Core;
using System.Linq;

namespace Todo.Core.Tests
{
    public class TodoListTests
    {
        [Fact]
        public void Add_IncreasesCount()
        {
            var list = new TodoList();
            list.Add(" task ");
            Assert.Equal(1, list.Count);
            Assert.Equal("task", list.Items.First().Title);
        }

        [Fact]
        public void Remove_ById_Works()
        {
            var list = new TodoList();
            var item = list.Add("a");
            Assert.True(list.Remove(item.Id));
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void Find_ReturnsMatches()
        {
            var list = new TodoList();
            list.Add("Buy milk");
            list.Add("Read book");
            var found = list.Find("buy").ToList();
            Assert.Single(found);
            Assert.Equal("Buy milk", found[0].Title);
        }

        [Fact]
        public void Save_SerializesItemsToJson()
        {
            // Arrange
            var list = new TodoList();
            var item1 = list.Add("Task 1");
            item1.MarkDone();
            var item2 = list.Add("Task 2");

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                list.Save(tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                var json = File.ReadAllText(tempFile);
                Assert.Contains("Task 1", json);
                Assert.Contains("Task 2", json);
                Assert.Contains("isDone", json);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Load_DeserializesItemsFromJson()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var originalList = new TodoList();
            var originalItem = originalList.Add("Test Task");
            originalItem.MarkDone();

            try
            {
                // Сохраняем
                originalList.Save(tempFile);

                // Act
                var loadedList = TodoList.Load(tempFile);

                // Assert
                Assert.Equal(1, loadedList.Count);
                var loadedItem = loadedList.Items.First();
                Assert.Equal(originalItem.Id, loadedItem.Id);
                Assert.Equal("Test Task", loadedItem.Title);
                Assert.True(loadedItem.IsDone);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Load_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => TodoList.Load(nonExistentFile));
        }

        [Fact]
        public void SaveAndLoad_PreservesAllProperties()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var originalList = new TodoList();

            originalList.Add("Task 1");
            var item2 = originalList.Add("Task 2");
            item2.MarkDone();
            var item3 = originalList.Add("Task 3");
            item3.Rename("Renamed Task 3");

            try
            {
                // Act
                originalList.Save(tempFile);
                var loadedList = TodoList.Load(tempFile);

                // Assert
                Assert.Equal(3, loadedList.Count);

                var loadedItems = loadedList.Items.ToList();
                Assert.Equal("Task 1", loadedItems[0].Title);
                Assert.False(loadedItems[0].IsDone);

                Assert.Equal("Task 2", loadedItems[1].Title);
                Assert.True(loadedItems[1].IsDone);

                Assert.Equal("Renamed Task 3", loadedItems[2].Title);
                Assert.False(loadedItems[2].IsDone);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadFromFile_WorksCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var list = new TodoList();
            list.Add("Task 1");

            // Act & Assert - сначала сохраняем
            list.Save(tempFile);

            // Создаем новый список и загружаем в него
            var newList = new TodoList();
            newList.LoadFromFile(tempFile);

            Assert.Equal(1, newList.Count);
            Assert.Equal("Task 1", newList.Items.First().Title);

            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}