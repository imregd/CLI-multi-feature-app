using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static WordDefinition;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

public class WordDefinition
{
    public class Definition
    {
        public string definition { get; set; }
        public List<object> synonyms { get; set; }
        public List<object> antonyms { get; set; }
        public string example { get; set; }
    }

    public class License
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Meaning
    {
        public string partOfSpeech { get; set; }
        public List<Definition> definitions { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> antonyms { get; set; }
    }

    public class Phonetic
    {
        public string audio { get; set; }
        public string sourceUrl { get; set; }
        public License license { get; set; }
        public string text { get; set; }
    }

    public class Root
    {
        public string word { get; set; }
        public List<Phonetic> phonetics { get; set; }
        public List<Meaning> meanings { get; set; }
        public License license { get; set; }
        public List<string> sourceUrls { get; set; }
    }
}





public class DictionaryAPI // main dictionary class
{

    //  class that does the dictionary api request
    private static HttpClient httpClient = new HttpClient(); // this is made because i got an error about httpclient being static, so this makes an instance of the object or smth
    public static async Task<List<WordDefinition.Root>> GetWordData(string word)
    {
        var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
        List<WordDefinition.Root> dictionaryAnswerHolder = new List<WordDefinition.Root>();
        dictionaryAnswerHolder = await httpClient.GetFromJsonAsync<List<WordDefinition.Root>>(url); // gets the data from the API and puts it into a list from a class to made to sort the data
        return dictionaryAnswerHolder;
    }




    // main dictionary method
    public static async Task Dictionary()
    {
        Console.WriteLine("This is MY dictionary app thing using FreeDictionaryAPI, follow prompts and get a word\n--------");
        Console.WriteLine("What word do you want the definition of?\n------------\nEnter: ");
        string? word = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(word))
        {
            Console.WriteLine("You must enter a word.");
        }
        else
        {

            try
            {
                WordDefinition helper = new WordDefinition(); // stops errors from happening as the class is like static or smth so an instance has to be made
                List<WordDefinition.Root> definitions = await GetWordData(word);
                foreach (var root in definitions) // i do NOT know why this works but chatgpt gave me it (to do with json format but still doesnt make any sense)
                {
                    foreach (var meaning in root.meanings)
                    {
                        foreach (var def in meaning.definitions)
                        {
                            Console.WriteLine($"----------\n{def.definition}");
                        }
                    }
                }


            }

            catch (HttpRequestException e) // if something went wrong with the request
            {

                Console.WriteLine($"Something went wrong :(\nWord is very most likely not found\nRequest error: {e.Message}");
            }
        }
    }
}

// ------------------------------------------------------------------------------------------------------
// flash card feature: 

public class Flashcard
{
    private string file = "flashcard.txt";
    private string datefile = "flashdate.txt";
    private Dictionary<string, string> FlashCardDict = new();
    private List<(string date, TimeSpan Duration)> FlashCardDates = new(); // key will be date (DD/MM/YYYY) value will be time spent (XX hour, XX minute and XX seconds), separated by colon in file
    bool keepRunning = true;
    int count = 0;
    int totalcount = 0;


    private void ViewFlashCards()
    {
        Console.Clear();
        if (FlashCardDict.Count == 0)
            {
            Console.WriteLine("No flashcard found :(\nAdd flashcards to the deck!");
            return;
        }
        else
        {
            foreach (var flashcard in FlashCardDict)
            {
                Console.WriteLine("----------");
                Console.WriteLine($"Question: {flashcard.Key}\nAnswer: {flashcard.Value}\n");
            }
        }
    }

    private void ViewDateHistory()
    {
        Console.Clear();
        if (FlashCardDates.Count == 0)
        {
            Console.WriteLine("No date found you lazy bum");
            return;
        }
        else
        {
            foreach(var index in FlashCardDates)
            {
                Console.WriteLine($"Date: {index.date} Time spent: {index.Duration.Hours} hour(s), {index.Duration.Minutes} minute(s) and {index.Duration.Seconds} second(s) ");
                Console.WriteLine("----------");
            }
        }
    }

    private void AddFlashcard()
    {
        Console.Clear();
        Console.WriteLine("Enter the question to add to flashcard: ");
        string? question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question))
        {
            Console.WriteLine("You must enter a question.");
            return;
        }
        Console.WriteLine("Enter the answer to add to flashcard: ");
        string? answer = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(answer))
        {
            Console.WriteLine("You must enter an answer.");
            return;
        }
        Console.WriteLine($"Add '{question}' with the answer '{answer}'? (Y/N) ");
        string? confirmation = Console.ReadLine();
        if (confirmation is not null && confirmation.ToLower() == "y")
        {
            FlashCardDict[question] = answer; 
            File.AppendAllText(file, $"\n{question}\n{answer}\n"); 
            Console.WriteLine("Flashcard added successfully!");
        }
        else
        {
            Console.WriteLine("Flashcard not added.");
        }

    }

    private void RemoveFlashcards(string[] lines)
    {
        Console.Clear();
        Console.WriteLine("Enter the question to remove from flashcard: ");
        string? question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question))
        {
            Console.WriteLine("You must enter a question.");
            return;
        }
        if (FlashCardDict.ContainsKey(question))
        {

            Console.WriteLine($"The question '{question}' alongside the answer '{FlashCardDict[question]}' has been removed, please make sure to quit the program via the built in exit feature for changes to save!");
            FlashCardDict.Remove(question);

        }
        else
        {
            Console.WriteLine("Question not found in flashcard deck");
        }
    }

    private void ClearFlashcards()
    {
        Console.Clear();
        Console.WriteLine("Are you sure you want to clear the flashcard deck? (Y/N)");
        string? confirmation = Console.ReadLine();
        if (confirmation is not null && confirmation.ToLower() == "y")
        {
            FlashCardDict.Clear(); 
            File.WriteAllText(file, ""); 
            Console.WriteLine("Flashcard deck cleared successfully!");
        }
        else
        {
            Console.WriteLine("Flashcard deck not cleared.");
        }
    }

    private void ClearDateHistory()
    {
        Console.Clear();
        Console.WriteLine("Are you sure you want to clear the date history? (y/n) <-- peak y/n reference");
        string? confirmation = Console.ReadLine();
        if (confirmation is not null && confirmation.ToLower() == "y")
        {
            FlashCardDates.Clear(); 
            File.WriteAllText(datefile, ""); 
            Console.WriteLine("Date history cleared successfully!");
        }
        else
        {
            Console.WriteLine("Date history not cleared.");
        }
    }

    private void PlayFlashCard()
    {
        Console.Clear();
        totalcount++;
        Console.WriteLine($"This is flashcard practice part! Your current score is {count} out of");
        if (FlashCardDict.Count == 0)
        {
            Console.WriteLine("No flashcards found, please add some first!");
            return;
        }
        Random random = new Random();
        var flashcardKeys = FlashCardDict.Keys.ToList();
        int randomIndex = random.Next(flashcardKeys.Count);
        string randomQuestion = flashcardKeys[randomIndex];
        Console.WriteLine($"Question: {randomQuestion}");
        Console.WriteLine("Enter answer: ");
        string? userAnswer = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(userAnswer))
        {
            Console.WriteLine("You must enter an answer.");
            return;
        }
        if (userAnswer.Equals(FlashCardDict[randomQuestion], StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Correct! 1 point added to score!");
            count++;
        }
        else
        {
            Console.WriteLine($"Incorrect! The correct answer is: {FlashCardDict[randomQuestion]}");
        }
        Console.WriteLine($"Answer: {FlashCardDict[randomQuestion]}");
    }

    public Flashcard()
    {
        DateTime currentDate = DateTime.Now;
        
        if (!File.Exists(file))
        {
            File.Create(file).Close(); // creates the file if it does not exist

        }
        string[] lines = File.ReadAllLines(file);
        if (lines.Length > 0)
        {
            for(int i = 0; i < lines.Length; i = i + 2)
            {
                if (i + 1 < lines.Length) 
                {
                    FlashCardDict[lines[i]] = lines[i + 1]; 
                }
            }

            Console.WriteLine("Flashcard file loaded!");

        }

        

        if (!File.Exists(datefile))
        {
            File.Create(datefile).Close();
        }
        else
        {
            string[] dirtyLines = File.ReadAllLines(datefile);
            string[] dateLines = dirtyLines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            if (dateLines.Length > 0)
            {
                // figure something out to add dates to list, datetime works but i need hours on the file, which datetime doesnt do, maybe  use 2 date times at start and end and subtract hours
                foreach(var x in dateLines)
                {
                    string[] parts = x.Split(',');
                    if (parts.Length == 2)
                    {
                        FlashCardDates.Add((parts[0], TimeSpan.Parse(parts[1])));
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("Flashcard date file successfully loaded!");
            }
        }
        Console.WriteLine("Welcome to flashcard app, Type the number in the brackets to select options");
        while(keepRunning)
        {
            Console.WriteLine("(1) View current flashcard deck\n(2) View all date history\n(3) Add to the flashcard deck\n(4) Remove from flashcard deck\n(5) Clear flashcard deck\n(6) Clear date history\n(7) Exit flashcard app\n(8) Play/Practice flashcards\nEnter: ");
            string? userInput = Console.ReadLine();
            if(userInput is not null)
            {
                switch(userInput)
                {
                    case "1":
                        ViewFlashCards();
                        break;
                    case "2":
                        ViewDateHistory();
                        break;
                    case "3":
                        AddFlashcard();
                        break;
                    case "4":
                        RemoveFlashcards(lines);
                        break;
                    case "5":
                        ClearFlashcards();
                        break;
                    case "6":
                        ClearDateHistory();
                        break;
                    case "7":
                        keepRunning = false;
                        Console.WriteLine("Exiting flashcard app...");

                        var lines4 = FlashCardDict.Select(index => $"{index.Key}\n{index.Value}");
                        File.WriteAllLines(file, lines4);

                        DateTime endDate = DateTime.Now;
                        TimeSpan timeSpent = endDate - currentDate;
                       Console.Write(FlashCardDates);
                        string formattedDateKey = endDate.ToString("dd/MM/yyyy");
                        FlashCardDates.Add((formattedDateKey, timeSpent));


                        var grouped = FlashCardDates.GroupBy(x => x.date);
                        var totallines = grouped.Select(group =>
                        {
                            TimeSpan total = group.Aggregate(TimeSpan.Zero, (acc, item) => acc + item.Duration);
                            return $"{group.Key},{total}\n";
                        });

                        File.WriteAllLines(datefile, totallines);

                        break;
                    case "8":
                        PlayFlashCard();
                        break;
                    default:
                        Console.WriteLine("Invalid input, please enter a number between 1 and 7.");
                        break;
                }
            }
        }    
        




    }

}


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to this program that has the following features (enter number 1/2/3/4):\n1. Dictionary\n2. Flashcard (CLI)\n3. Speed typer (wip)\n4. Quit\nEnter: ");
        var UserInput = Console.ReadLine();
        switch (UserInput)
        {
            case "1":
                Console.Clear();
                DictionaryAPI.Dictionary().GetAwaiter().GetResult();
                break;

            case "2":
                Console.Clear();
                Flashcard flashcardApp = new Flashcard();
                break;
            case "3":
                Console.Clear();
                Console.WriteLine("Speed typer feature is not implemented yet.");
                break;
            case "4":
                break;
            default:
                Console.Clear();
                Console.WriteLine("Invalid input, please enter a number between 1 and 4.");
                break;


        }


            
    }

}



