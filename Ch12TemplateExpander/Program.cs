using Ch12TemplateExpander;

var dom = """
          <html>
            <body>
              <ul z-loop="item:names">
                <li><span z-var="item"/></li>
              </ul>
            </body>
          </html>
          """;

Dictionary<string, Data> data = new()
{
    ["names"] = new ArrayData(["Johnson", "Vaughan", "Jackson"])
};