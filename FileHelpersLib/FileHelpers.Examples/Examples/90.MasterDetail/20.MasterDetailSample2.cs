﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using FileHelpers.MasterDetail;

namespace ExamplesFramework
{
    /// <summary>
    /// Example of the Master Detail processing
    /// </summary>
    public class SecondMasterDetailSample
        : ExampleBase
    {

        //-> {Example.Name:Master Detail sample}
        //-> {Example.Description:Show how to implement Master detail reading where record contains characters}

        //-> {Example.File:RunEngine.cs}
        /// <summary>
        /// Run a record through engine using a Common selector where master contains a characrter to create a master detail input
        /// </summary>
        public override void Run()
        {
            var engine = new MasterDetailEngine<Customers, Orders>
                                         (CommonSelector.MasterIfContains, "@");
            // to Read use:
            var res = engine.ReadFile("Input.txt");

            // to Write use:
            engine.WriteFile("Output.txt", res);
        }
        //-> {/Example.File}

        //-> {Example.File:Input.txt}
        /*@ALFKI|Alfreds Futterkiste|Maria Anders|Sales Representative|Obere Str. 57|Berlin|Germany
        10248|ALFKI|5|04071996|01081996|16071996|3|32.38
        10249|ALFKI|6|05071996|16081996|10071996|1|11.61
        10251|ALFKI|3|08071996|05081996|15071996|1|41.34
        @ANATR|Ana Trujillo Emparedados y helados|Ana Trujillo|Owner|Avda. de la Constitución 2222|México D.F.|Mexico
        10252|ANATR|4|09071996|06081996|11071996|2|51.3
        @ANTON|Antonio Moreno Taquería|Antonio Moreno|Owner|Mataderos  2312|México D.F.|Mexico
        */
        //-> {/Example.File}

        //-> {Example.File:Output.txt}
        //-> {/Example.File}

        //-> {Example.File:Master layout.cs}
        /// <summary>
        /// Layout of the master records beginning with alpha characters in input
        /// </summary>
        [DelimitedRecord("|")]
        [IgnoreEmptyLines]
        public class Customers
        {
            public string CustomerID;
            public string CompanyName;
            public string ContactName;
            public string ContactTitle;
            public string Address;
            public string City;
            public string Country;
        }
        //-> {/Example.File}

        //-> {Example.File:Detail layout.cs}
        /// <summary>
        /// Layout of the detail records beginning with numerics in input
        /// </summary>
        [DelimitedRecord("|")]
        public class Orders
        {
            public int OrderID;
            public string CustomerID;
            public int EmployeeID;
            public DateTime OrderDate;
            public DateTime RequiredDate;
            public DateTime ShippedDate;
            public int ShipVia;
            public decimal Freight;
        }
        //-> {/Example.File}
    }
}