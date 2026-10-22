using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Визначаємо правильний шлях до папки Xml (вона лежить у корені WebModule1)
string currentDir = Directory.GetCurrentDirectory();
string xmlDir = Path.GetFullPath(Path.Combine(currentDir, "../Xml"));

// Якщо програма запускається з папки bin/Debug, шлях буде іншим
if (!Directory.Exists(xmlDir))
{
    xmlDir = Path.GetFullPath(Path.Combine(currentDir, "../../../../Xml"));
}

string xmlPath = Path.Combine(xmlDir, "catalog.xml");
string xsdPath = Path.Combine(xmlDir, "catalog.xsd");

// Перевірка, чи взагалі існують файли за цим шляхом
if (!File.Exists(xmlPath))
{
    Console.WriteLine($"КРИТИЧНА ПОМИЛКА: Файл не знайдено за шляхом {xmlPath}");
    Console.WriteLine("Переконайтеся, що папка Xml з файлами лежить у WebModule1!");
    return;
}

Console.WriteLine("--- 1. Перевірка XSD (включаючи xs:unique) ---");
ValidateXml(xsdPath, xmlPath);

Console.WriteLine("\n--- 2. Читання через XmlDocument (DOM) ---");
var doc = new XmlDocument();
doc.Load(xmlPath);
var resources = doc.SelectNodes("//resource");
Console.WriteLine($"Знайдено ресурсів: {resources?.Count}");

Console.WriteLine("\n--- 3. Читання через XDocument (LINQ) ---");
var xdoc = XDocument.Load(xmlPath);
var skus = from r in xdoc.Descendants("resource")
           select r.Element("sku")?.Value;
Console.WriteLine($"Всі SKU: {string.Join(", ", skus)}");

Console.WriteLine("\n--- 4. Читання через XmlReader (Потоковий) ---");
using (var reader = XmlReader.Create(xmlPath))
{
    while (reader.Read())
    {
        if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
        {
            Console.WriteLine($"Знайдено назву: {reader.ReadElementContentAsString()}");
        }
    }
}


// Метод валідації за XSD
static void ValidateXml(string schemaFile, string xmlFile)
{
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit, // Забороняємо DTD
        XmlResolver = null,
        ValidationType = ValidationType.Schema
    };

    settings.Schemas.Add(null, schemaFile);

    bool isValid = true;
    settings.ValidationEventHandler += (s, e) =>
    {
        isValid = false;
        Console.WriteLine($"ПОМИЛКА ВАЛІДАЦІЇ: {e.Message}");
    };

    try
    {
        using var reader = XmlReader.Create(xmlFile, settings);
        while (reader.Read()) { }
        if (isValid) Console.WriteLine("Успіх: Документ валідний за XSD!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Критична помилка: {ex.Message}");
    }
}