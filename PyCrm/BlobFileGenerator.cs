using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYCRM

{
    class BlobFileGenerator
    {
        public static void WriteToBlob(CustomerBluePrint person)
        {
            string HeaderString;
            string EntryString;
            try
            {
                HeaderString = ("name,number,email,birthday,birthmonth,annivday,annivmonth,agegroup,ethnicity,area,pin,city,state,referredby,remark\n");
                EntryString = person.Name + "," + person.MobileNumber + "," + person.Email + "," + person.BirthDay + "," + person.BirthMonth + "," + person.AnniversaryDay + "," + person.AnniversaryMonth + "," + person.AgeGroup + "," + person.Ethnicity + "," + person.Area + "," + person.PinCode + "," + person.City + "," + person.State + "," + person.ReferredBy + "," + person.Remark;
                string csvFileName = fileNameCreator.csvFileNameCreatetor("CustDetails");
                string FileData = HeaderString + EntryString;
                System.IO.File.WriteAllText(csvFileName, FileData);
            }
            catch (Exception e)
            {

            }
            try
            {
                EntryString = person.Name + "," + person.MobileNumber + "," + person.Email + "," + person.BirthDay + "," + person.BirthMonth + "," + person.AnniversaryDay + "," + person.AnniversaryMonth + "," + person.AgeGroup + "," + person.Ethnicity + "," + person.Area + "," + person.PinCode + "," + person.City + "," + person.State + "," + person.ReferredBy + "," + person.Remark;
                string csvFileName = fileNameCreator.csvFileNameCreatetor("CustDetails");
                string FileData = EntryString;
                System.IO.File.WriteAllText(csvFileName, FileData);
            }
            catch (Exception e)
            {

            }

        }
    }
}
