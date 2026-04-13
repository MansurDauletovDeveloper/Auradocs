using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace DocumentFlow.Controllers
{
    public class XmlController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public XmlController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        // Task 1: Read XML with XmlReader
        public IActionResult Task1()
        {
            var filePath = Path.Combine(_env.WebRootPath, "xml", "Data.xml");
            var documents = new List<Dictionary<string, string>>();

            using (XmlReader reader = XmlReader.Create(filePath))
            {
                Dictionary<string, string>? currentDoc = null;
                string currentElement = "";

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            currentElement = reader.Name;
                            if (currentElement == "Document")
                            {
                                currentDoc = new Dictionary<string, string>();
                            }
                            break;
                        case XmlNodeType.Text:
                            if (currentDoc != null && !string.IsNullOrEmpty(currentElement))
                            {
                                currentDoc[currentElement] = reader.Value;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "Document" && currentDoc != null)
                            {
                                documents.Add(currentDoc);
                                currentDoc = null;
                            }
                            currentElement = "";
                            break;
                    }
                }
            }

            return View(documents);
        }

        // Task 2: LINQ to XML Filtering
        public IActionResult Task2(string roleFilter = "admin")
        {
            var filePath = Path.Combine(_env.WebRootPath, "xml", "Users.xml");
            
            // Load using LINQ to XML
            XDocument xdoc = XDocument.Load(filePath);
            
            // Filter
            var filteredUsers = xdoc.Descendants("User");
            
            if (!string.IsNullOrEmpty(roleFilter))
            {
                filteredUsers = filteredUsers.Where(u => u.Element("Role")?.Value == roleFilter);
            }

            var usersList = filteredUsers.Select(u => new UserXmlModel
            {
                Name = u.Element("Name")?.Value ?? "",
                Email = u.Element("Email")?.Value ?? "",
                Role = u.Element("Role")?.Value ?? ""
            }).ToList();

            ViewBag.CurrentFilter = roleFilter;
            return View(usersList);
        }

        // Task 3: XSLT Transformation
        public IActionResult Task3()
        {
            var xmlPath = Path.Combine(_env.WebRootPath, "xml", "Data.xml");
            var xsltPath = Path.Combine(_env.WebRootPath, "xml", "Style.xslt");

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltPath);

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
                {
                    xslt.Transform(xmlPath, xmlWriter);
                }
                
                string resultHtml = stringWriter.ToString();
                ViewBag.HtmlResult = resultHtml;
            }

            return View();
        }

        // Task 4: XML + ADO.NET + web
        public IActionResult Task4()
        {
            var exportPath = Path.Combine(_env.WebRootPath, "xml", "ExportedDocuments.xml");
            var connString = _config.GetConnectionString("DefaultConnection");

            // 1. Export DB Data to XML
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var query = "SELECT Id, Title, Content, CreatedAt, AuthorId FROM Documents";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    var xmlSettings = new XmlWriterSettings
                    {
                        Indent = true,
                        Async = false
                    };
                    using (var writer = XmlWriter.Create(exportPath, xmlSettings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Documents");

                        while (reader.Read())
                        {
                            writer.WriteStartElement("Document");
                            writer.WriteElementString("Id", reader["Id"].ToString());
                            writer.WriteElementString("Title", reader["Title"].ToString());
                            writer.WriteElementString("Content", reader["Content"]?.ToString());
                            writer.WriteElementString("CreatedAt", Convert.ToDateTime(reader["CreatedAt"]).ToString("o"));
                            writer.WriteElementString("AuthorId", reader["AuthorId"]?.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
            }

            // 2. Read back for display
            var docsList = new List<Dictionary<string, string>>();
            using (XmlReader xmlReader = XmlReader.Create(exportPath))
            {
                Dictionary<string, string>? currentDoc = null;
                string currentElement = "";
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        currentElement = xmlReader.Name;
                        if (currentElement == "Document") currentDoc = new Dictionary<string, string>();
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Text)
                    {
                        if (currentDoc != null && !string.IsNullOrEmpty(currentElement))
                        {
                            currentDoc[currentElement] = xmlReader.Value;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        if (xmlReader.Name == "Document" && currentDoc != null)
                        {
                            docsList.Add(currentDoc);
                            currentDoc = null;
                        }
                        currentElement = "";
                    }
                }
            }

            return View(docsList);
        }

        // Task 5: Validation and XSD Schema
        [HttpGet]
        public IActionResult Task5()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Task5(Microsoft.AspNetCore.Http.IFormFile uploadedXml)
        {
            if (uploadedXml == null || uploadedXml.Length == 0)
            {
                ViewBag.Error = "Please select an XML file to upload.";
                return View();
            }

            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                uploadedXml.CopyTo(stream);
            }

            var schemaPath = Path.Combine(_env.WebRootPath, "xml", "Schema.xsd");
            
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", schemaPath);

            var validationErrors = new List<string>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas = schemas;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (sender, args) =>
            {
                validationErrors.Add($"[{args.Severity}] {args.Message}");
            };

            try
            {
                using (XmlReader reader = XmlReader.Create(tempFilePath, settings))
                {
                    while (reader.Read()) { } // Read through the whole document to validate
                }

                if (validationErrors.Any())
                {
                    ViewBag.ValidationErrors = validationErrors;
                    System.IO.File.Delete(tempFilePath);
                    return View();
                }

                // Validation successful, apply XSLT transformation
                var xsltPath = Path.Combine(_env.WebRootPath, "xml", "UploadStyle.xslt");
                if (System.IO.File.Exists(xsltPath))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(xsltPath);

                    using (StringWriter stringWriter = new StringWriter())
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
                        {
                            xslt.Transform(tempFilePath, xmlWriter);
                        }
                        ViewBag.TransformedHtml = stringWriter.ToString();
                    }
                }
                else
                {
                    ViewBag.Message = "XML uploaded and validated successfully (XSLT not found).";
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add(ex.Message);
                ViewBag.ValidationErrors = validationErrors;
            }
            finally
            {
                System.IO.File.Delete(tempFilePath);
            }

            return View();
        }

        // Task 6: Data Synchronization via XML
        [HttpGet]
        public IActionResult Task6()
        {
            var dataPath = Path.Combine(_env.WebRootPath, "xml", "DataUpload.xml");
            var directory = Path.GetDirectoryName(dataPath);
            if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            if (!System.IO.File.Exists(dataPath))
            {
                var defaultContent = "<Root>\n  <Record>\n    <Id>1</Id>\n    <Name>Test Record 1</Name>\n    <Value>100</Value>\n  </Record>\n  <Record>\n    <Id>2</Id>\n    <Name>Test Record 2</Name>\n    <Value>200</Value>\n  </Record>\n</Root>";
                System.IO.File.WriteAllText(dataPath, defaultContent);
            }
            
            XDocument xdoc = XDocument.Load(dataPath);
            var records = xdoc.Descendants("Record").Select(r => new Task6Record
            {
                Id = r.Element("Id")?.Value ?? "",
                Name = r.Element("Name")?.Value ?? "",
                Value = r.Element("Value")?.Value ?? ""
            }).ToList();

            return View(records);
        }

        [HttpPost]
        public IActionResult Task6Sync(string id, string newName, string newValue)
        {
            var dataPath = Path.Combine(_env.WebRootPath, "xml", "DataUpload.xml");
            
            // Edit in memory using XmlDocument
            XmlDocument doc = new XmlDocument();
            doc.Load(dataPath);

            XmlNode? targetNode = null;
            var nodes = doc.SelectNodes("//Record");
            if (nodes != null)
            {
                foreach (XmlNode record in nodes)
                {
                    if (record["Id"]?.InnerText == id)
                    {
                        targetNode = record;
                        break;
                    }
                }
            }

            if (targetNode != null && targetNode["Name"] != null && targetNode["Value"] != null)
            {
                targetNode["Name"]!.InnerText = newName;
                targetNode["Value"]!.InnerText = newValue;
                doc.Save(dataPath); // Save the edited XML back

                // Log change
                LogChange(id, newName, newValue);
                TempData["Message"] = "Record updated and synced successfully.";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }

            return RedirectToAction("Task6");
        }

        private void LogChange(string id, string newName, string newValue)
        {
            var logPath = Path.Combine(_env.WebRootPath, "xml", "ChangeLog.xml");
            XDocument logDoc;

            if (System.IO.File.Exists(logPath))
            {
                logDoc = XDocument.Load(logPath);
            }
            else
            {
                logDoc = new XDocument(new XElement("ChangeLog"));
            }

            var userName = User.Identity?.IsAuthenticated == true ? User.Identity?.Name ?? "System" : "System";
            var newEntry = new XElement("Change",
                new XElement("Timestamp", DateTime.UtcNow.ToString("o")),
                new XElement("User", userName),
                new XElement("RecordId", id),
                new XElement("NewName", newName),
                new XElement("NewValue", newValue)
            );

            logDoc.Root?.Add(newEntry);
            logDoc.Save(logPath);
        }

        [HttpGet]
        public IActionResult Task6History(string userFilter, DateTime? dateFilter)
        {
            var logPath = Path.Combine(_env.WebRootPath, "xml", "ChangeLog.xml");
            if (!System.IO.File.Exists(logPath))
            {
                return View(new List<ChangeLogEntry>());
            }

            XDocument logDoc = XDocument.Load(logPath);
            var query = logDoc.Descendants("Change").Select(c => new ChangeLogEntry
            {
                Timestamp = DateTime.Parse(c.Element("Timestamp")?.Value ?? DateTime.MinValue.ToString()),
                User = c.Element("User")?.Value ?? "",
                RecordId = c.Element("RecordId")?.Value ?? "",
                NewName = c.Element("NewName")?.Value ?? "",
                NewValue = c.Element("NewValue")?.Value ?? ""
            }).AsQueryable();

            if (!string.IsNullOrEmpty(userFilter))
            {
                query = query.Where(c => c.User != null && c.User.Contains(userFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(c => c.Timestamp.Date == dateFilter.Value.Date);
            }

            var entries = query.OrderByDescending(e => e.Timestamp).ToList();

            ViewBag.UserFilter = userFilter;
            ViewBag.DateFilter = dateFilter?.ToString("yyyy-MM-dd");

            return View(entries);
        }
    }

    public class Task6Record
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ChangeLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UserXmlModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
