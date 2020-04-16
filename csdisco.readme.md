# CSDISCO Documentation

## Development on Mac

1. Use vs code
1. Need to setup artifactory for nuget on mac.  See [link](https://csdisco.atlassian.net/wiki/spaces/~carr/pages/325124153/Artifactory).
1. Need to install nuget.  See [link](https://csdisco.atlassian.net/wiki/spaces/~carr/pages/325124153/Artifactory#Artifactory-.net)

Once you have that, then do following:

1. git clone this repo
1. in terminal go to repo directory and run `nuget restore net-core/Ical.Net.sln`
1. open project directory in vs code - all symbols should be recognized, tests should be runnable

If you opened the project first, and then ran `nuget restore ...`, you may need to restart omnisharp.
    1. <command+shift> + <p> 
    1. Type `restart omnisharp` and execute it
    1. restart vs code after it's done

### Testing using vs code

Tests are located in `{proj-root}/net-core/Ical.Net.CoreUnitTests`

#### Running an individual test:
In vs-code you can run a test individually in the IDE by navigating to the test method.

#### Running all tests

##### Easiest
`dotnet test {proj-root}/net-core/Ical.Net.CoreUnitTests/`

##### Exactly what Jenkins does
1. Install powershell (`brew cask install powershell`) -- Needed if you want to run test suite from commandline
1. `cd {proj-root}/net-core`
1. `pwsh test.ps1`

##### Within vs code
1. Install [c# .NET test explorer extension](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
1. Point the test explorer to the .NET tests:
    1. <command> <comma>
    1. Click "Workspace" to switch to workspace settings
    1. Enter "Test Project Path"
    1. Set this to net-core/Ical.Net.CoreUnitTests/Ical.Net.CoreUnitTests.csproj

Click test icon to the left (looks like a chemistry flask), and you can run tests.

# Original iCal Documentation

./readme.md