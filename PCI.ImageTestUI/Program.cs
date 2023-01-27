﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCI.ImageTestUI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check Connection
            bool status = Bootstrapper.CheckConnection();
            if (!status)
            {
                MessageBox.Show("Cannot establish the connection to the server, make sure the IP Server and Port Reachable, the app will close!");
                Environment.Exit(0);
            }

            // Connect to Network
            Bootstrapper.ConnectDirectoryServer();

            // Dependency injection
            var containerBuilder = Bootstrapper.DependencyInjectionBuilder(new ContainerBuilder());
            var container = containerBuilder.Build();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(container.Resolve<Main>());
        }
    }
}