# SpeedySchema

## Overview
This is a tool that will allow you to create a schema with some elegant crafted short-hand syntax. 

This code would create a CREATE statement for a table called "Contacts"

> ::Contact>*Id, Firstname, LastName, DOB(DateTime?), Age(int), Middlename(text[10]?)"

The application would parse this input and output a T-SQL statement to create the table. 

You can also add a switch to generate and execute the statement in one call.

## Syntax Overview

There are 4 parts to a *speedy* schema query: 

1. **Operation** - Presently there are only two operations supported: `CREATE TABLE` and `DROP TABLE`.  The create statement will start with a double **colon** (::) and the drop statement will start with a double caret (^^).
2. **Table Name** - Immediately after the operation is the name of the table followed by a greater than symbol (>).
3. **Field List** - Immediately after the table name is the field list.  This is a comma-delimited set of fields.  Commas are not supported in field names.  Spaces and other punctuation supported by SQL Server will be supported indirectly. 
4. **Field Decorators** - coming soon
