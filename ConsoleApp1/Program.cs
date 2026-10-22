using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Надійний метод пошуку папки Xml (піднімається від папки bin до самого кореня)
string FindXmlDir()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        string p = Path.Combine(dir.FullName, "Xml");
        if (Directory.Exists(p) && File.Exists(Path.Combine(p, "catalog.xml")))
            return p;
        dir = dir.Parent;
    }
    return null!;
}

string xmlDir = FindXmlDir();
if (xmlDir == null)
{
    Console.WriteLine("КРИТИЧНА ПОМИЛКА: Папку Xml з файлом catalog.xml не знайдено!");
    return;
}

string xmlPath = Path.Combine(xmlDir, "catalog.xml");
string xsdPath = Path.Combine(xmlDir, "catalog.xsd");

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

static void ValidateXml(string schemaFile, string xmlFile)
{
    var settings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Prohibit,
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