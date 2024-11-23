# Windows Comminication Foundataion (WCF) and Web Service (WS) test client application
While I'm working with Web Service I have to write test client for testing this services, including checking for version conflicts. When one of the clients told me that service "not working" bug I can send him a test client to exclude problems with web service or give me exact trace data to determine exact exception location.

When I start to write WCF services, I was glad that in the package with VS there is a tool named WCF Test Client. After that there is no need of writing tests by hand, but not convenient to add services and test data by hand each time.

After I scan working process of WCF Test Client, I decided to figure out how it works from the inside...