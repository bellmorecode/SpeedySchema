SpeedySchema
============

This is a tool that will allow you to create a schema with some elegant crafted short-hand syntax. 

This code would create a CREATE statement for a table called "Contacts"

> ::Contact>*Id, Firstname, LastName, DOB(DateTime?), Age(int), Middlename(text[10]?)"

The application would parse this input and output a T-SQL statement to create the table. 

You can also add a switch to generate and execute the statement in one call.

