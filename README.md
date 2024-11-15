**English Version**

* * * * *

**GitHub Source Copier**

**GitHub Source Copier** is a lightweight application for retrieving source files from a GitHub repository, transforming them, and saving them into a local project with updated namespaces and version compatibility.

**Features**

-   Fetch source files from GitHub repositories.
-   Update namespaces dynamically based on local directory structure.
-   Replace old technology versions with the new ones in the code.
-   Recursively process subdirectories and files.
-   Fully asynchronous API.
-   Integrated with Swagger for testing and documentation.

* * * * *

**Technologies Used**

-   **.NET 8**
-   **ASP.NET Core Minimal APIs**
-   **C# 11**
-   **Swagger/OpenAPI**
-   **HttpClient**

* * * * *

**Installation**

1.  Clone the repository:

- git clone https://github.com/<your-repo>/GithubSourceCopier.git

cd GithubSourceCopier

- Restore the dependencies and build the project:

- dotnet restore

dotnet build

- Run the project:

3. dotnet run

* * * * *

**API Usage**

**Endpoint: ****/api/project-updater/update-project**

**Method**: POST

**Request Body** (JSON):

{

 "gitHubLink": "https://github.com/username/repository/tree/main/SourceFolder",

 "localPath": "C:\\Projects\\DestinationFolder",

 "oldVersion": ".NET 6",

 "newVersion": ".NET 8",

 "targetNamespace": "MyProject.Core"

}

**Response** (JSON):

{

 "Message": "All files have been added successfully.",

 "AddedFiles": [

 "C:\\Projects\\DestinationFolder\\Example1.cs",

 "C:\\Projects\\DestinationFolder\\SubFolder\\Example2.cs"

 ]

}

* * * * *

**Swagger Integration**

After running the project, navigate to:

https://localhost:<port>/swagger

You can test the endpoint directly using the Swagger interface.

* * * * *

**Azerbaijani Version**

* * * * *

**GitHub Mənbə Köçürmə Tətbiqi**

**GitHub Mənbə Köçürmə Tətbiqi** GitHub repository-dən mənbə faylları götürmək, onları transformasiya etmək və yerli layihəyə saxlamaq üçün yüngül bir proqramdır. Namespace və versiya uyğunluğunu təmin edir.

**Xüsusiyyətlər**

-   GitHub repository-dən mənbə faylları əldə edir.
-   Namespace-ləri yerli qovluq strukturuna əsasən avtomatik yeniləyir.
-   Köhnə texnologiya versiyalarını yeni versiyalarla əvəz edir.
-   Alt qovluqları və faylları rekursiv şəkildə emal edir.
-   Tamamilə asinxron işləyir.
-   Swagger ilə inteqrasiya olunmuşdur.

* * * * *

**İstifadə Edilən Texnologiyalar**

-   **.NET 8**
-   **ASP.NET Core Minimal API-lər**
-   **C# 11**
-   **Swagger/OpenAPI**
-   **HttpClient**

* * * * *

**Qurulum**

1.  Repository-i klonlayın:

- git clone https://github.com/<your-repo>/GithubSourceCopier.git

cd GithubSourceCopier

- Asılılıqları bərpa edin və layihəni qurun:

- dotnet restore

dotnet build

- Layihəni işə salın:

3. dotnet run

* * * * *

**API İstifadəsi**

**Endpoint: ****/api/project-updater/update-project**

**Metod**: POST

**Sorğu Gövdəsi** (JSON):

{

 "gitHubLink": "https://github.com/username/repository/tree/main/SourceFolder",

 "localPath": "C:\\Projects\\DestinationFolder",

 "oldVersion": ".NET 6",

 "newVersion": ".NET 8",

 "targetNamespace": "MyProject.Core"

}

**Cavab** (JSON):

{

 "Message": "Bütün fayllar uğurla əlavə edildi.",

 "AddedFiles": [

 "C:\\Projects\\DestinationFolder\\Example1.cs",

 "C:\\Projects\\DestinationFolder\\SubFolder\\Example2.cs"

 ]

}

* * * * *

**Swagger İnteqrasiyası**

Layihəni işə saldıqdan sonra aşağıdakı URL-ə keçid edin:

https://localhost:<port>/swagger

Endpoint-i birbaşa Swagger interfeysi vasitəsilə test edə bilərsiniz.
