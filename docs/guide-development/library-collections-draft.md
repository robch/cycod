# 4. Collections

```csharp
// Organized bookshelves (List<T>) for items in sequence
var favoriteBooks = new List<string> { "1984", "Dune", "Foundation" };
var customerAges = new List<int> { 25, 34, 28, 45 };

// Card catalog system (Dictionary<K,V>) for quick lookups by reference number
var booksByAuthor = new Dictionary<string, List<string>>
{
    ["Asimov"] = new List<string> { "Foundation", "I, Robot" },
    ["Herbert"] = new List<string> { "Dune", "Dune Messiah" },
    ["Orwell"] = new List<string> { "1984", "Animal Farm" }
};

// Special collection ensuring no duplicate titles (HashSet<T>)
var uniqueGenres = new HashSet<string> { "Science Fiction", "Fantasy", "Mystery" };
var readBookTitles = new HashSet<string> { "Dune", "Foundation", "1984" };

// Empty library sections instead of closed libraries
var emptyBookshelf = new List<Book>();              // Empty but ready for books
var emptyCardCatalog = new Dictionary<string, Book>(); // Empty but organized

// Creating a copy of a library collection for lending
var originalLibrary = new List<Book> { book1, book2, book3 };
var lendingCopy = new List<Book>(originalLibrary);   // Copy for safe lending

// Read-only library catalog for public access
public IReadOnlyList<Book> GetAvailableBooks()
{
    // Patrons can read the catalog but can't modify the library's collection
    return _libraryBooks.AsReadOnly();
}

public IReadOnlyDictionary<string, int> GetBookCounts()
{
    // Public can see book counts but can't change inventory
    return _bookInventory.AsReadOnly();
}

// Specialized library organization systems
var bookQueue = new Queue<Book>();           // Book return processing line
var bookStack = new Stack<Book>();           // Recently returned books pile
var bookArray = new Book[100];               // Fixed-size rare book display case

// Adding books to different library organization systems
favoriteBooks.Add("Neuromancer");           // Add to bookshelf
booksByAuthor["Gibson"] = new List<string> { "Neuromancer" }; // Add to card catalog
uniqueGenres.Add("Cyberpunk");              // Add to genre collection (no duplicates)

// Library operations - checking what's available
var hasSciFi = uniqueGenres.Contains("Science Fiction");
var asimovBooks = booksByAuthor.ContainsKey("Asimov") ? booksByAuthor["Asimov"] : new List<string>();
var totalBooks = favoriteBooks.Count;

// Library processing workflows
bookQueue.Enqueue(returnedBook);             // Add to return processing line
var nextBookToProcess = bookQueue.Dequeue(); // Take next book from line

bookStack.Push(newAcquisition);              // Add to new arrivals pile
var latestBook = bookStack.Pop();            // Take top book from pile

// Searching through library collections
var longBooks = allBooks.Where(book => book.PageCount > 300);
var booksByDecade = allBooks.GroupBy(book => book.PublicationYear / 10 * 10);
var authorNames = allBooks.Select(book => book.Author).Distinct();

// Combining library collections
var combineBooks = fiction.Concat(nonFiction).ToList();
var commonAuthors = scienceFiction.Intersect(fantasy).ToList();

// Library collection validation
if (requestedBooks.Any())
{
    ProcessBookRequest(requestedBooks);
}

var firstAvailableBook = availableBooks.FirstOrDefault();
var totalValue = rareBooks.Sum(book => book.EstimatedValue);
```

### Core Principles

- Choose the right library organization system based on how you'll access your books:
  - Bookshelves (List<T>) for ordered collections you'll browse sequentially
  - Card catalogs (Dictionary<K,V>) for quick lookups by author, title, or topic
  - Special collections (HashSet<T>) when you need to prevent duplicate titles
- Always provide empty library sections rather than closing the library (return empty collections, not null)
- Create lending copies of collections when others need to modify them safely
- Provide read-only catalogs for public access to protect your library's organization
- Use collection initializers for setting up small library sections
- Consider the library patron's workflow when choosing organization systems

### Why It Matters

Think of collections as different library organization systems - each designed for specific ways of storing, finding, and accessing books. Just as a well-organized library makes it easy to find the right book quickly, well-chosen collection types make your data easy to access and manage.

Using the wrong collection type is like organizing a library poorly - putting reference books in random order, or using a card catalog system when you just need a simple bookshelf.

Well-organized library systems provide:

1. **Efficient Access**: The right organization system lets you find books quickly
2. **Clear Organization**: Each system has a clear purpose and access pattern
3. **Safe Lending**: Copies protect the original collection while allowing others to work with the data
4. **Public Access**: Read-only catalogs let others browse without disrupting organization
5. **Appropriate Capacity**: Different systems handle different collection sizes and access patterns

When collections are chosen poorly, it's like a library where fiction is mixed with reference books, the card catalog is unsorted, and patrons can't find anything they need.

### Common Mistakes

#### Using the Wrong Library Organization System

```csharp
// BAD: Using a dictionary when you just need a simple bookshelf
var bookOrder = new Dictionary<int, string>
{
    [0] = "First Book",
    [1] = "Second Book", 
    [2] = "Third Book"
};

// BAD: Using a list when you need quick author lookups
var allBooks = new List<(string Author, string Title)>();
// Now you have to search through every book to find an author
var dostoyevskyBooks = allBooks.Where(book => book.Author == "Dostoevsky").ToList();
```

**Why it's problematic**: This is like using a complex card catalog system to store three books in order, or organizing thousands of books on shelves when you frequently need to find all books by specific authors. The organization system doesn't match the access pattern.

**Better approach**:

```csharp
// GOOD: Simple bookshelf for ordered collection
var bookOrder = new List<string> { "First Book", "Second Book", "Third Book" };

// GOOD: Card catalog system for author lookups
var booksByAuthor = new Dictionary<string, List<string>>
{
    ["Dostoevsky"] = new List<string> { "Crime and Punishment", "The Brothers Karamazov" },
    ["Tolstoy"] = new List<string> { "War and Peace", "Anna Karenina" }
};
var dostoyevskyBooks = booksByAuthor["Dostoevsky"]; // Quick lookup
```

#### Closing the Library Instead of Providing Empty Sections

```csharp
// BAD: Closing the library when no books are available
public List<Book> GetBooksInGenre(string genre)
{
    var booksInGenre = _library.Where(book => book.Genre == genre).ToList();
    
    if (!booksInGenre.Any())
        return null; // Library closed! Patrons can't even browse empty sections
        
    return booksInGenre;
}

// This forces library patrons to check if the library is closed
var books = GetBooksInGenre("Mystery");
if (books != null) // Have to check if library is open
{
    foreach (var book in books) { /* ... */ }
}
```

**Why it's problematic**: This is like closing the entire library when the mystery section is empty, rather than just showing patrons an empty but organized section. Patrons now have to check if the library is open before they can even look around.

**Better approach**:

```csharp
// GOOD: Always keep the library open with organized sections
public List<Book> GetBooksInGenre(string genre)
{
    var booksInGenre = _library.Where(book => book.Genre == genre).ToList();
    return booksInGenre; // Returns empty list if no books, but library stays open
}

// Patrons can always browse, even if sections are empty
var books = GetBooksInGenre("Mystery");
foreach (var book in books) { /* Works even with empty collection */ }
```

#### Allowing Patrons to Reorganize the Main Library

```csharp
// BAD: Giving patrons direct access to reorganize the library
public class Library
{
    public List<Book> MainCollection;  // Anyone can add, remove, or reorder books!
    
    public List<Book> GetBooks()
    {
        return MainCollection; // Direct access to main library organization
    }
}

// Patrons can accidentally mess up the library organization
var library = new Library();
var books = library.GetBooks();
books.Clear(); // Oops! Accidentally cleared the entire library!
```

**Why it's problematic**: This is like letting library patrons reorganize the main collection, move books between sections, or accidentally take all the books home. The library's organization becomes unreliable.

**Better approach**:

```csharp
// GOOD: Provide read-only catalogs for public access
public class Library
{
    private readonly List<Book> _mainCollection;  // Protected library organization
    
    public IReadOnlyList<Book> GetBooks()
    {
        return _mainCollection.AsReadOnly(); // Patrons can browse but not reorganize
    }
    
    public List<Book> GetBooksForCheckout(List<string> titles)
    {
        // Controlled book lending with copies
        return _mainCollection.Where(book => titles.Contains(book.Title)).ToList();
    }
}
```

#### Using Complex Organization for Simple Book Collections

```csharp
// BAD: Over-engineering organization for a few books
public class PersonalBookshelf
{
    private Dictionary<string, Dictionary<int, Dictionary<string, Book>>> _books;
    
    public void AddBook(Book book)
    {
        // Complex cataloging system for just a few personal books
        if (!_books.ContainsKey(book.Genre))
            _books[book.Genre] = new Dictionary<int, Dictionary<string, Book>>();
            
        if (!_books[book.Genre].ContainsKey(book.PublicationYear))
            _books[book.Genre][book.PublicationYear] = new Dictionary<string, Book>();
            
        _books[book.Genre][book.PublicationYear][book.Title] = book;
    }
}
```

**Why it's problematic**: This is like creating a complex multi-level cataloging system with genre, year, and title indices for a personal collection of 10 books. The organization system is more complex than the collection warrants.

**Better approach**:

```csharp
// GOOD: Simple organization for simple collections
public class PersonalBookshelf
{
    private readonly List<Book> _books = new List<Book>();
    
    public void AddBook(Book book)
    {
        _books.Add(book); // Simple bookshelf organization
    }
    
    public List<Book> GetBooksByGenre(string genre)
    {
        return _books.Where(book => book.Genre == genre).ToList();
    }
}
```

### Evolution Example

Let's see how collection usage might evolve from poor library organization to excellent collection management:

**Initial Version - Chaotic library with no organization:**

```csharp
// Initial version - poor library organization
public class LibrarySystem
{
    // No clear organization system
    public object[] stuff;  // Books mixed with other items
    
    public object FindBook(string title)
    {
        // Have to search through everything to find one book
        for (int i = 0; i < stuff.Length; i++)
        {
            if (stuff[i].ToString().Contains(title))
                return stuff[i];
        }
        return null; // Library closed if book not found
    }
    
    public void AddBook(object book)
    {
        // No organized way to add books
        // Have to create new array every time
        var newStuff = new object[stuff.Length + 1];
        stuff.CopyTo(newStuff, 0);
        newStuff[stuff.Length] = book;
        stuff = newStuff;
    }
}
```

**Intermediate Version - Some organization but still issues:**

```csharp
// Better organization but still problematic
public class LibrarySystem
{
    private List<Book> books;  // Better than array but still limited organization
    
    public List<Book> FindBooksByAuthor(string author)
    {
        // Still searching through all books for author
        var result = new List<Book>();
        foreach (var book in books)
        {
            if (book.Author == author)
                result.Add(book);
        }
        return result.Count > 0 ? result : null; // Still closing library for empty results
    }
    
    public void AddBook(Book book)
    {
        books.Add(book); // Better adding but no duplicate checking
    }
}
```

**Final Version - Excellent library organization system:**

```csharp
// Excellent library organization with appropriate collection types
public class LibrarySystem
{
    // Different organization systems for different access patterns
    private readonly List<Book> _allBooks = new List<Book>();
    private readonly Dictionary<string, List<Book>> _booksByAuthor = new Dictionary<string, List<Book>>();
    private readonly Dictionary<string, List<Book>> _booksByGenre = new Dictionary<string, List<Book>>();
    private readonly HashSet<string> _bookTitles = new HashSet<string>(); // Prevent duplicates
    
    // Processing queues for library operations
    private readonly Queue<Book> _newAcquisitions = new Queue<Book>();
    private readonly Stack<Book> _recentReturns = new Stack<Book>();
    
    // Read-only public access to library catalogs
    public IReadOnlyList<Book> AllBooks => _allBooks.AsReadOnly();
    public IReadOnlyDictionary<string, List<Book>> BooksByAuthor => 
        _booksByAuthor.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsReadOnly().ToList())
            .AsReadOnly();
    
    public void AddBook(Book book)
    {
        // Prevent duplicate titles using special collection
        if (_bookTitles.Contains(book.Title))
        {
            Console.WriteLine($"Book '{book.Title}' already in library collection");
            return;
        }
        
        // Add to all appropriate organization systems
        _allBooks.Add(book);
        _bookTitles.Add(book.Title);
        
        // Update card catalog by author
        if (!_booksByAuthor.ContainsKey(book.Author))
            _booksByAuthor[book.Author] = new List<Book>();
        _booksByAuthor[book.Author].Add(book);
        
        // Update genre organization
        if (!_booksByGenre.ContainsKey(book.Genre))
            _booksByGenre[book.Genre] = new List<Book>();
        _booksByGenre[book.Genre].Add(book);
        
        // Add to processing queue
        _newAcquisitions.Enqueue(book);
    }
    
    public List<Book> GetBooksByAuthor(string author)
    {
        // Quick lookup using card catalog system
        return _booksByAuthor.ContainsKey(author) 
            ? new List<Book>(_booksByAuthor[author])  // Return copy for safe lending
            : new List<Book>();  // Empty collection, not null
    }
    
    public List<Book> GetBooksByGenre(string genre)
    {
        return _booksByGenre.ContainsKey(genre)
            ? new List<Book>(_booksByGenre[genre])
            : new List<Book>();
    }
    
    public bool HasBook(string title)
    {
        // Quick duplicate check using special collection
        return _bookTitles.Contains(title);
    }
    
    public void ProcessNewAcquisitions()
    {
        // Process books from acquisition queue
        while (_newAcquisitions.Count > 0)
        {
            var book = _newAcquisitions.Dequeue();
            CatalogBook(book);
        }
    }
    
    public Book GetMostRecentReturn()
    {
        // Get most recently returned book from stack
        return _recentReturns.Count > 0 ? _recentReturns.Pop() : null;
    }
}
```

### Deeper Understanding

#### Library Organization Principles

Good collection choice follows the same principles as good library organization:

1. **Access Pattern Matching**: Choose organization systems based on how you'll find and retrieve items
2. **Efficient Browsing**: Some systems are for sequential browsing, others for direct lookup
3. **Capacity Planning**: Different systems handle different collection sizes efficiently
4. **Public vs. Private Access**: Control what patrons can do vs. what librarians can do

#### Collection Types as Library Systems

1. **Bookshelf Organization (List<T>)**:
   ```csharp
   var bookshelf = new List<Book>();  // Ordered, indexed, allows duplicates
   ```
   Best for: Sequential access, maintaining order, small to medium collections

2. **Card Catalog System (Dictionary<K,V>)**:
   ```csharp
   var catalog = new Dictionary<string, Book>();  // Key-value lookup
   ```
   Best for: Fast lookups by key, large collections, no duplicate keys

3. **Special Collections (HashSet<T>)**:
   ```csharp
   var uniqueTitles = new HashSet<string>();  // No duplicates, fast membership testing
   ```
   Best for: Ensuring uniqueness, fast existence checking, set operations

4. **Processing Lines (Queue<T>)**:
   ```csharp
   var processingLine = new Queue<Book>();  // First in, first out
   ```
   Best for: Sequential processing, fair ordering, task queues

5. **Recent Items Pile (Stack<T>)**:
   ```csharp
   var recentReturns = new Stack<Book>();  // Last in, first out
   ```
   Best for: Most recent access, undo operations, temporary storage

#### Collection Safety and Access Control

**Read-Only Catalogs**:
```csharp
public IReadOnlyList<Book> GetBooks() => _books.AsReadOnly();
```
Like providing a catalog that patrons can browse but not modify.

**Safe Lending Copies**:
```csharp
public List<Book> GetBooksForProject() => new List<Book>(_books);
```
Like creating copies of rare books for researchers to work with.

**Empty Sections vs. Closed Libraries**:
```csharp
// Always return organized empty sections
return new List<Book>();  // Not null
```
Like keeping library sections organized even when they're temporarily empty.

Good collection design makes your data as accessible and well-organized as a world-class library system.