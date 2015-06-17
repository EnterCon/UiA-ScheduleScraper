# UiA-ScheduleGrabber <image src="https://ci.appveyor.com/api/projects/status/087lgi4ryvgbx5x1?svg=true&passingText=master%20-%20OK">

Application for parsing the UiA schedule information for all departments.
It's a simple application which produces JSON from
the [UiA schedule webpage](http://timeplan.uia.no/swsuiah/public/no/default.aspx).

# Usage

Windows:
```sh
ScheduleGrabber.exe --help

   ____    __          __     __    _____         __   __
  / __/___/ /  ___ ___/ /_ __/ /__ / ___/______ _/ /  / /  ___ ____
 _\ \/ __/ _ \/ -_) _  / // / / -_) (_ / __/ _ `/ _ \/ _ \/ -_) __/
/___/\__/_//_/\__/\_,_/\_,_/_/\__/\___/_/  \_,_/_.__/_.__/\__/_/

Usage: ScheduleGrabber [OPTIONS]
Write UiA schedule data to JSON
If no flags are given, departments.json is generated.

Options:
  -h, --help                 show this message and exit
  -f, --file=VALUE           write JSON to specified file
  -i, --id=VALUE             write only the specified ID to JSON file
```

# Examples

This will save the schedule for IT and Information Systems third year to ITSchedule.json
in the executables directory:
```sh
ScheduleGrabber.exe -f ITSchedule.json -i #SPLUS83F11B
```

# Features

I am considering the following features in the future:

* XML-support
* Adapters for No-SQL schemaless databases? (--db mongodb --host localhost etc..)


# License
MIT, see [license](https://github.com/martinothamar/UiA-ScheduleGrabber/blob/master/LICENSE).
