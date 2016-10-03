# OpenKonnect
## Intro
OpenKonnect is a tool useful to fetch stamps from card-readers in a multi-threaded fashion. OpenKonnect instantiates an independent thread for each card-reader it is up to fetch stamps from, periodically and concurrently. Once fetced, stamps are stored into a DBMS in a reliable fashion. Moreover, OpenKonnect can take in charge card-reader's clock adjustments as well. Basically, it can handle as many readers as you want.

![Kronotech readers](https://github.com/supix/openkonnect/blob/master/AllReaders.jpg)

It is implemented as Microsoft Windows Service and uses .NET framework v4.0. OpenKonnect supports Kronotech readers and, so far, MySql database. OpenKonnect is released under the [GPL-3.0 license](https://www.gnu.org/licenses/gpl-3.0.en.html).

## Installation
* Download [the latest release](https://github.com/supix/openkonnect/releases).
* Execute the setup.
* Configure the application (see the next section).
* Start OpenKonnect windows service.

## OpenKonnect Configuration
### Readers configuration
All the readers you want to pool have to be listed in the configuration file. By default, configuration file is called `openkonnect.conf` and is located in the same directory as the application. Each row defines a reader. The configuration file syntax is the following.

```
# this is a comment
; this is a comment as well

R001  reader001.domain.com  192.168.0.1        120 # comments can be also here
R002  reader002.domain.com  192.168.0.2        120
R003  reader003.domain.com  192.168.0.3:12345  120 # this reader listens on port 12345, the others on the default port (i.e. 3000)
R003  reader003.domain.com  192.168.0.3          0 # this zero-interval defaults to configuration file value
...
```

Each line contains:
* the reader code (will appear in the logs and in the DB table)
* the reader DNS name
* the reader IP address and possibly the port (default port is 3000)
* the pool interval, in seconds; a zero interval defaults to the value contained within the configuration file
* possibly a comment

### Configuring the database
OpenKonnect stores stamps into a MySql database. After having created the database instance, the table hosting stamps can be created with the following DDL statement:

```sql
CREATE TABLE `stamps` (
	`ID` INT(11) NOT NULL AUTO_INCREMENT,
	`STAMPTIME` TIMESTAMP,
	`DIRECTION` CHAR(1),
	`REASON` INT(11),
	`CARDCODE` VARCHAR(100),
	`READERCODE` VARCHAR(100),
	`INSERTIONTIME` TIMESTAMP,
	PRIMARY KEY (`ID`)
);
```
A user must be created with grants to insert in the table. His credentials must be indicated in the connection string (see the next section). Obviously you can create further (nullable) columns and/or secondary-indexes on this table, according to your needs.

### Other parameters
All the other parameters are contained in the configuration file, called `OpenKonnectService.exe.config`. It is an XML file. The main configuration parameters are contained in the following section, containing useful comments and some reasonable defaults for each value.

```xml
<appSettings>
    <!-- connection string of the DB where stamps are stored -->
    <add key="ConnectionString" value="Server=localhost;Database=openkonnect;Uid=openkonnect;Pwd=openkonnect;" />

    <!-- if true, card-readers are not actually contacted (they can even not exist), but fake stamps are returned.
    Useful to test just the db connection. -->
    <add key="FakeMode" value="true" />

    <!-- if true, only the first stamp stored in a card-reader is returned on each job execution,
    and it is not deleted from the reader. Useful to test the application without altering card reader state. 
    Otherwise, all available stamps are fetched and deleted from the reader on each job execution. -->
    <add key="SafeMode" value="true" />

    <!-- card reader configuration filename -->
    <add key="CardReaderConfigurationFileName" value="openkonnect.conf" />

    <!-- interval in seconds between two garbage collection cycles -->
    <add key="GarbageCollectorInterval_sec" value="600" />

    <!-- default interval in seconds between two card-reader connections: this interval is used for readers
    having a zero-interval in the configuration file. -->
    <add key="FetchDefaultInterval_sec" value="300" />

    <!-- if true card-reader clocks are set through a scheduled job -->
    <add key="UpdateClocks_Active" value="true" />

    <!-- time of day (format: hhmmss) when card-reader clocks are updated -->
    <add key="UpdateClocks_TimeOfDay" value="020200" />

    <!-- interval in seconds between two card-reader clocks update -->
    <add key="UpdateClocks_Interval_sec" value="86400" />

    <!-- timespan in milliseconds within card-reader clock updates are spread (to avoid network spikes)-->
    <add key="UpdateClocks_WithinTime_msec" value="10000" />
  </appSettings>
```

`FakeMode` and `SafeMode` parameters deserve special attention.

#### FakeMode
If `FakeMode` is true, readers configuration is loaded from configuration file, but readers are never actually contacted. Fake stamps are returned and stored into the database. This mode allows to test the application (expecially the database connection) without involving actual readers.

#### SafeMode
If `FakeMode` is false and `SafeMode` is true, configured reader are contacted through the network, but only the first stamp stored in the reader is read and isn't deleted from the reader. This mode allows to test the whole system without altering readers state.

Obviously, in production, `FakeMode` and `SafeMode` have to be set to false.

## How it works
When the service is started, it instantiates a scheduled job for each reader. Jobs are executed at regular time intervals, but the start time of each job is selected randomically, in order to spread network traffic across the timeline, so avoiding network spikes.

When a job starts, all the stored stamps are fetched and marked as deleted (actually, Kronotech readers logically mark stamps as deleted, but they can still be accessed with the proprietary software using the appropriate function). OpenKonnect fetches stamps one-by-one. After a stamp has been fetched it is inserted into the database. Only on successful insertions, the stamp on the reader is deleted and the next one is fetched. This assures that no stamps can be lost in case of database and/or network failures.

When readers' clock adjust-task is active, a further job is instantiated for each reader. Tasks start at the time of day indicated in the configuration file, spread over the indicated time interval (again, to avoid network congestion). The default daytime is 02:02, for the sake of adjusting clocks just after the daylight saving time switch. The task is (by default) executed every 86.400 seconds (i.e. every day at the same time).

## Log
During the execution, the service logs about every important information:
* reader connections
* fetched stamps (with all their data)
* exceptions (e.g. network failures, db failures, etc.)
* service stop/start
* number of instantiated jobs

The log is configured in the `log4net` section within the configuration file `OpenKonnectService.exe.config`. By default, log can reach a maximum size of 10MB. Then, it rotates and a new file is created. A maximum of 100 files is retained. After waiting enough time, you will find the following log files:
```
openkonnect.log
openkonnect.log.1
openkonnect.log.2
openkonnect.log.3
...
openkonnect.log.100
```
All these parameters are configurable in the indicated section. In case you want to log debug information, you can set the loglevel to `DEBUG` (it is `INFO` by default). See [log4net manual](https://logging.apache.org/log4net/release/manual/configuration.html) for more information.

By default, the logfile is saved at the following path: `%APPDATA%\openkonnect\openkonnect.log`. `%APPDATA%` is the data folder belonging to the user the service is execute as. For instance, if the service is executed under Windows 7+ by the user `foo`, `%APPDATA%` is `C:\Users\foo\AppData\Roaming`.

## Troubleshooting
Sometimes, the service shows an error just after it is started. Often, this is due to lack of authorizations and can be solved configuring a specific user executing the service.

Should the software raise an exception during the connection with readers, perhaps readers are not configured as expected. In this case it could be necessary to tune the protocol by modifying the code and recompiling the application.

## Source code
The source code is available at https://github.com/supix/openkonnect. OpenKonnect is written in C# language using Visual Studio CE 2013 and the following contributions from the open-source world:
* [quartz.net](http://www.quartz-scheduler.net/) library for task scheduling
* [log4net](https://logging.apache.org/log4net/) library for logging
* [faker.net.portable](https://github.com/AdmiringWorm/Faker.NET.Portable) library to generate fake stamps data

The `Setup` folder contains a [InnoSetup](http://www.jrsoftware.org/isinfo.php) project. Compiling this project the executable installation program is generated.

OpenKonnect - 2016 - Contact the author at: esposito.marce [at] gmail.com<br/>
OpenKonnect has nothing to do with Kronotech enterprise. OpenKonnect is provided as is without any guarantees or warranty. In association with the product, the author makes no warranties of any kind, either express or implied. Use of the product by a user is at the user’s risk.
