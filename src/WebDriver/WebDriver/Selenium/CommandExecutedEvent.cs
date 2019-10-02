using System;

namespace WebDriver.Functions.Selenium
{
    public delegate void CommandEvent(object sender, CommandEventArgs e);

    public class CommandEventArgs : EventArgs
    {
        public Command Command { get; private set; }
        public CommandEventArgs(Command command) => Command = command;
    }
}
