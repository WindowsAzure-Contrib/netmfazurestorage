using System;
using netmfazurestorage.Account;
using netmfazurestorage.Table;
using System.Collections;
using Microsoft.SPOT;
using System.Net;

namespace netmfazurestorage.Tests
{
    public class TableTests
    {
        TableClient client;

        public TableTests(string account, string key)
        {
            this.client = new TableClient(new CloudStorageAccount(account,key));
        }

        public void Run()
        {
            this.TestCreate();
            this.TestInsert();
            this.TestInsertDouble();
            this.TestInsertExperimental();
            this.QuerySingleEntity();
            this.QueryMultipleEntities();
            this.UpdateTableEntity();
        }

        private void TestCreate()
        {
            this.client.CreateTable("netmftest");

        }

        public void TestInsert()
        {
            var values = new System.Collections.ArrayList();
            var tep = new TableEntityProperty();
            tep.Name = "field1";
            tep.Value = "value1";
            tep.Type = "Edm.String";

            values.Add(tep);

            tep = new TableEntityProperty();
            tep.Name = "field2";
            tep.Value = "value2";
            tep.Type = "Edm.String";
            values.Add(tep);

            var code = this.client.InsertTableEntity("netmftest", "1", Guid.NewGuid().ToString(), DateTime.Now, values);
            Debug.Assert(HttpStatusCode.Created == code);
        }

        public void TestInsertDouble()
        {
            var values = new System.Collections.ArrayList();
            var tep = new TableEntityProperty();
            tep.Name = "field1";
            tep.Value = "value1";
            tep.Type = "Edm.String";

            values.Add(tep);

            tep = new TableEntityProperty();
            tep.Name = "field2";
            tep.Value = "value2";
            tep.Type = "Edm.String";
            values.Add(tep);

            tep = new TableEntityProperty();
            tep.Name = "field3";
            tep.Value = "5.0";
            tep.Type = "Edm.Double";
            values.Add(tep);

            var code = this.client.InsertTableEntity("netmftest", "1", Guid.NewGuid().ToString(), DateTime.Now, values);
            Debug.Assert(HttpStatusCode.Created == code);
        }


        public void TestInsertExperimental()
        {
            var values = new Hashtable();
            values.Add("guidfield", Guid.NewGuid());
            values.Add("int32field", 32);
            values.Add("stringfield", "string");
            //values.Add("datetimefield", DateTime.Now); // not sure why this is appearing as a null in the table
            values.Add("doublefield", (double)123.22);
            values.Add("int64field", (Int64)64);
            values.Add("boolfield", true);
            var code = this.client.InsertTableEntity("netmftest", "2", Guid.NewGuid().ToString(), DateTime.Now, values);
            Debug.Assert(code == System.Net.HttpStatusCode.Created);
        }

        public void QuerySingleEntity()
        {
            var output = this.client.QueryTable("netmftest", "2", "796440bd-95f6-0626-975b-764e6902844d");
            Debug.Assert(null != output);
        }

        public void QueryMultipleEntities()
        {
            var output = this.client.QueryTable("netmftest", "PartitionKey eq '2'");
            Debug.Assert(null != output);
            Debug.Assert(output.Count > 0);
        }

        public void UpdateTableEntity()
        {
            var rowKey = Guid.NewGuid();
            var values = new Hashtable();
            values.Add("guidfield", Guid.NewGuid()); 
            values.Add("int32field", 32);
            values.Add("stringfield", "string");
            //values.Add("datetimefield", DateTime.Now); // not sure why this is appearing as a null in the table
            values.Add("doublefield", (double)123.22);
            values.Add("int64field", (Int64)64);
            values.Add("boolfield", true);
            var code1 = this.client.InsertTableEntity("netmftest", "3", rowKey.ToString(), DateTime.Now, values);

            values["stringfield"] = "updated string";
            var code2 = this.client.UpdateTableEntity("netmftest", "3", rowKey.ToString(), DateTime.Now, values);
        }

    }

}