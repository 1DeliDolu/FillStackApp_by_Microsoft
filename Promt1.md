### **Activity Introduction**

Imagine you are part of a development team tasked with building InventoryHub, an inventory management system for a small business. The application must allow users to view product details from a back-end API. Your goal is to create the integration between the front-end (Blazor) and the back-end (Minimal API).

In this activity, you’ll use Microsoft Copilot to generate and refine integration code that ensures seamless communication between the two components.

This is the first of four activities that build towards the InventoryHub application. The integration code you create here will form the foundation for debugging and performance optimization in later activities.

# Activity Instructions

## Step 1:  Set up the provided base application code

### Create your applications and a solution file

1. Browse to a root folder in which you want to create your application.
2. Create your application folder and move to it with the following commands: **mkdir FullStackAppcd FullStackApp**
3. Create your Client and Server applications using the following commands: **dotnet new blazorwasm -n ClientAppdotnet new webapi -n ServerApp**
4. Create a solution and add your applications to it with the following commands: **dotnet new sln -n FullStackSolutiondotnet sln add ClientApp ServerApp**
5. Replace the code in the **ServerApp's****Program.cs** with this starter code:

**Minimal API Back-End (ServerApp.cs):**

**var builder = WebApplication.CreateBuilder(args);**

**var app = builder.Build();**

**app.MapGet("/api/products", () =>**

**{**

**    return new[]**

**    {**

**        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },**

**        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }**

**    };**

**});**

**app.Run();**

### Open and launch your applications

1. Open two new terminal windows in VS Code.
2. In the first terminal window, use **cd ClientApp** to move to your client app directory.
3. In the second terminal window, use **cd ServerApp** to move to your server app directory.
4. Run the **Blazor ClientApp** and **ServerApp** projects using dotnet run in each terminal window.
5. Open the ClientApp (front end) in your browser. Notice that the data is not displayed because the integration code has not implemented yet.
6. Open the ServerApp (back end) in your browser and browse to **http://localhost:[port number]/api/products**. Notice that the API returns product information in JSON format.

## Step 2: Generate integration code using Copilot

You need to fetch product data from the back-end API and display it in the front-end. Start by creating a new Blazor component for the product list.

1. In the front-end project, create a file called FetchProducts.razor and add the following starter code:

@page "/fetchproducts"

<h3>Product List</h3>

<ul>
   @if (products != null)
    {
        foreach (var product in products
        {
            <li>@product.Name - $@product.Price</li>
        }
    }
    else
    {
        <li>Loading...</li>
    }
</ul>

@code {
    private Product[]? products;
    protected override async Task OnInitializedAsync()
    {
        // API call logic will go here
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}

2. Use Microsoft Copilot to generate API call logic inside the OnInitializedAsync method:

* Use HttpClient to call the /api/products endpoint.
* Deserialize the JSON response into the Product class.

## Step 3: Refine and test the integration code

1. Refine the API call logic with Copilot to:
   * Add error handling for invalid API responses or timeouts.
   * Ensure the code adheres to best practices for readability and maintainability.
2. Run both the front-end and back-end projects to test the integration:
   * Verify that the product data appears in the browser.

## Step 4: Save your work

By the end of this activity, you will have generated and refined integration code that successfully retrieves product data from the Minimal API and displays it in the Blazor front-end.

Save your work for use in later activities. This work will be expanded in Activity 2, where you’ll debug and resolve integration issues.
