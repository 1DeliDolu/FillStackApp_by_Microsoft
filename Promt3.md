
### **Activity Introduction**

The front-end of InventoryHub relies on well-structured JSON responses from the back-end API. Your task in this activity is to ensure the API returns properly formatted JSON that supports the application’s requirements. You’ll use Microsoft Copilot to create and validate JSON structures.

This is the third of four activities in the capstone project. You’ll optimize performance and consolidate all your work to complete the project in the next activity.


### **Activity Instructions**

## Step 1:  Review the scenario

The back-end of **InventoryHub** must return product data in a standardized JSON format. The response should include:

* Product details such as id, name, price, and stock.
* A nested category object for each product.

## Step 2: Generate JSON structures using Copilot

1. In the Minimal API, modify the /api/productlist endpoint to include a nested category object:

   app.MapGet("/api/productlist", () =>
   {
   return new[]
   {
   new
   {
   Id = 1,
   Name = "Laptop",
   Price = 1200.50,
   Stock = 25,
   Category = new { Id = 101, Name = "Electronics" }
   },
   new
   {
   Id = 2,
   Name = "Headphones",
   Price = 50.00,
   Stock = 100,
   Category = new { Id = 102, Name = "Accessories" }
   }
   };
   });
2. Use Copilot to refine and validate the JSON structure to ensure it meets industry standards.

## Step 3: Test the JSON response

Use a tool like Postman to test the /api/productlist endpoint. Confirm that:

* The JSON response includes the required fields (id, name, price, stock, and category).
* The structure is valid and properly nested.

## Step 4: Save your work

By the end of this activity, you will have implemented well-structured JSON responses for the back-end API and verified their correctness.

Save your work for the final activity, where you’ll optimize your code and write a reflection on the coding process with Copilot.
