using System.Text.Json;

namespace Note_Manager__with__File_System_Monitoring;

internal class Program
{
    private static readonly string notesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotesData");
    private static readonly string logFilePath = Path.Combine(notesDir, "activity.log");

    static void Main()
    {
        Directory.CreateDirectory(notesDir);

        StartFileWatcher();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("--- Note Manager ---");
            Console.WriteLine("1. Add Note");
            Console.WriteLine("2. List Notes");
            Console.WriteLine("3. View Note");
            Console.WriteLine("4. Delete Note");
            Console.WriteLine("5. Exit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": AddNote(); break;
                case "2": ListNotes(); break;
                case "3": ViewNote(); break;
                case "4": DeleteNote(); break;
                case "5": return;
                default: Console.WriteLine("Invalid choice."); break;
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }
    public class Note
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    static void AddNote()
    {
        Console.Write("Enter title: ");
        string title = Console.ReadLine();
        Console.Write("Enter content: ");
        string content = Console.ReadLine();

        var note = new Note
        {
            Title = title,
            Content = content,
            CreatedAt = DateTime.Now
        };

        string filePath = Path.Combine(notesDir, $"{title}.json");

        try
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(note, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Note saved.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving note: {ex.Message}");
        }
    }

    static void ListNotes()
    {
        var files = Directory.GetFiles(notesDir, "*.json");
        if (files.Length == 0)
        {
            Console.WriteLine("No notes found.");
            return;
        }

        foreach (var file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                var note = JsonSerializer.Deserialize<Note>(json);
                Console.WriteLine($"- {note.Title} (Created: {note.CreatedAt})");
            }
            catch
            {
                Console.WriteLine($"Could not read file: {Path.GetFileName(file)}");
            }
        }
    }

    static void ViewNote()
    {
        ListNotes();
        Console.Write("Enter title of note to view: ");
        string title = Console.ReadLine();
        string filePath = Path.Combine(notesDir, $"{title}.json");

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Note not found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var note = JsonSerializer.Deserialize<Note>(json);
            Console.WriteLine($"\nTitle: {note.Title}\nCreated At: {note.CreatedAt}\nContent:\n{note.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading note: {ex.Message}");
        }
    }

    static void DeleteNote()
    {
        ListNotes();
        Console.Write("Enter title of note to delete: ");
        string title = Console.ReadLine();
        string filePath = Path.Combine(notesDir, $"{title}.json");

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Note not found.");
            return;
        }

        try
        {
            File.Delete(filePath);
            Console.WriteLine("Note deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting note: {ex.Message}");
        }
    }
    static void RenameNote()
    {
        ListNotes();
        Console.Write("Enter the title of the note to rename: ");
        string oldTitle = Console.ReadLine();
        string oldPath = Path.Combine(notesDir, $"{oldTitle}.json");

        if (!File.Exists(oldPath))
        {
            Console.WriteLine("Note not found.");
            return;
        }

        Console.Write("Enter the new title: ");
        string newTitle = Console.ReadLine();
        string newPath = Path.Combine(notesDir, $"{newTitle}.json");

        if (File.Exists(newPath))
        {
            Console.WriteLine("A note with the new title already exists.");
            return;
        }

        try
        {
            File.Move(oldPath, newPath);

            string json = File.ReadAllText(newPath);
            var note = JsonSerializer.Deserialize<Note>(json);
            note.Title = newTitle;
            File.WriteAllText(newPath, JsonSerializer.Serialize(note, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine("Note renamed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming note: {ex.Message}");
        }
    }

    static void StartFileWatcher()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(notesDir, "*.json");

        watcher.Created += OnChanged;
        watcher.Changed += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnRenamed;

        watcher.EnableRaisingEvents = true;
    }

    static void OnChanged(object sender, FileSystemEventArgs e)
    {
        string logEntry = $"{DateTime.Now}: {e.ChangeType} - {e.Name}";
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }

    static void OnRenamed(object sender, RenamedEventArgs e)
    {
        string logEntry = $"{DateTime.Now}: Renamed - From {e.OldName} to {e.Name}";
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
}
