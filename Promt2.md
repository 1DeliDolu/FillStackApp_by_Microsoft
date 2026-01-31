### **Activity Introduction**

After implementing the initial integration code for InventoryHub, issues have surfaced. Users are reporting that the product list does not display properly due to errors such as incorrect API routes, CORS restrictions, and malformed JSON responses. Your task is to debug and resolve these issues using Microsoft Copilot.

This is the second of four activities in the project. Debugging the integration code here ensures a stable foundation for JSON structuring in the next activity. 


### **Activity Instructions**

## Step 1:  Review the scenario

You are the lead developer for **InventoryHub**, tasked with resolving the following issues in the integration:

1. **Incorrect API route**: The front-end is calling /api/products, but the back-end endpoint has been updated to /api/productlist.
2. **CORS errors**: Security restrictions are preventing the front-end from accessing the back-end API.
3. **Malformed JSON**: A back-end developer accidentally modified the API response structure, breaking the front-end display.

Your goal is to debug and resolve these issues to restore functionality.

## Step 2: Debug integration issues using Copilot

1. **Fix the API route**:
   * Use Copilot to update the API call in FetchProducts.razor to match the updated back-end route (/api/productlist).
2. **Resolve CORS errors**:
   1. Add the following CORS configuration to the Minimal API’s Program.cs using Copilot:

      app.UseCors(policy =>
      policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader());
   2. Test the application to ensure the front-end can successfully access the back-end.
3. **Handle malformed JSON:**
   1. Use Copilot to implement error handling for JSON deserialization in the front-end.
   2. Add a try-catch block in the OnInitializedAsync method to handle invalid JSON responses:

      try
      {
      var response = await HttpClient.GetAsync("/api/productlist");
      response.EnsureSuccessStatusCode();
      var json = await response.Content.ReadAsStringAsync();
      products = JsonSerializer.Deserialize<Product[]>(json);
      }
      catch (Exception ex)
      {
      Console.WriteLine($"Error: {ex.Message}");
      }

## Step 3: Test the resolved code

Run the application:

* Verify that the product list is displayed correctly in the front-end.
* Ensure that no CORS errors or JSON-related exceptions are logged in the browser console.

## Step 4: Save your work

By the end of this activity, you will have debugged and resolved integration issues, ensuring seamless communication between the front-end and back-end.

Save your work for use in later activities. This work will be expanded in Activity 3, where you’ll implement JSON structures.
