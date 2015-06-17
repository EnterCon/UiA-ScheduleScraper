# UiA-ScheduleGrabber <image src="https://ci.appveyor.com/api/projects/status/087lgi4ryvgbx5x1?svg=true&passingText=master%20-%20OK">

Application for parsing the UiA schedule information for all departments.
It's a simple application in which should produce JSON from
the [UiA schedule webpage](http://timeplan.uia.no/swsuiah/public/no/default.aspx).

# Features

Currently the application only parses all departments and writes it to a JSON file.
I am considering the following features in the future:

* Grab data for all departments by default, or pass in ID if you only need 1.
 * For example, the current ID for IT and Information Systems third year is `#SPLUS83F11B`. This could happen by passing an --id flag perhaps.
* Flag for specifying JSON file to write to. (--file ?)
* XML-support
* Adapters for No-SQL schemaless databases? (--db mongodb --host localhost etc..)


# License
MIT
