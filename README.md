# Squares V1
Squares V1 was my first try at this problem. You can check the repo here: https://github.com/TadasBr/Squares

I spent about 4 hours on this first version. The plan is to use the remaining time (~4 hours) to finish Squares V2, mainly focusing on:
* Building a RESTful API with a clean structure
* Implementing and polishing the main functionalities

# Squares V2
## Completion
### Functional
- [x] As a user can import a list of points
- [x] I as a user can add a point to an existing list 
- [x] I as a user can delete a point from an existing list
- [x] I as a user can retrieve the squares identified

### Non-fuctional
- [x] Include prerequisites and steps to launch in `README`
- [x] The solution code must be in a `git` repository
- [x] The API should be implemented using .NET Core framework (ideally the newest stable version)
- [x] The API must have some sort of persistance layer
- [x] After sending a request the consumer shouldn't wait more than 5 seconds for a response

### Bonus points stuff!
- [x] RESTful API
- [x] Documentation generated from code (hint - `Swagger`)
- [x] Automated tests
- [ ] Containerization/deployment (hint - `Docker`)
- [x] Performance considerations
- [ ] Measuring SLI's
- [x] Considerations for scalability of the API
- [x] Comments/thoughts on the decisions you made

## What's my plan to finish the Squares V2 solution with remaining time?
### 1 Finish the RESTful API functionality:
* Complete the remaining methods
* Update endpoints to follow resource-based naming
* Return proper HTTP status codes
* Add input validations

### 2 I will refactor the code to follow best practices. Specifically:
* Move validation logic out of the repository and into the service layer
* Handle input validation in the controllers

### 3 Add a minimal set of automated tests to cover the core functionality, especially square detection:
* Positive cases for all methods
* Extra tests for squares since it's the main feature

### 4 Optimize the squares identification process (this is the most computation-heavy part):
* Use caching to avoid recalculating squares for the same set of points
* Try concurrent processing to speed up calculations for larger datasets
* Make sure optimizations are safe and don't break correctness
* Aim to keep API response times under 5 seconds even for bigger point sets

## Code explanations
### Controllers and Endpoints
**PointsController.cs** handles all CRUD operations for points (GET, POST, PUT, DELETE, and bulk import). The squares endpoint is currently part of this controller as GET /api/points/squares.

I thought about making a separate SquaresController with GET /api/squares, but since squares are derived from points and not stored in the database, it felt more natural to keep them here.

### In-memory database
I didn't set up a Dockerfile this time since I haven't done that before and wanted a quicker setup. Instead, I used an in-memory database.

One limitation is that I couldn't enforce unique constraints at the database level. That meant I had to spend a bit more time checking for duplicate points in the service layer.

Because of In-memory database you could say that this API is not stateless.

## How to run
Simply open the solution and run it as an HTTP application. The documentation (Swagger) should open automatically. If it doesn't, navigate to: http://localhost:5229/swagger/index.html