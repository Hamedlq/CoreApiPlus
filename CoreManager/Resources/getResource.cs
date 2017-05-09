using System;

namespace CoreManager.Resources
{
    public class getResource
    {
        //static private System.Resources.ResourceManager stringsResourceMan;
        //static private System.Resources.ResourceManager messageResourceMan;
        //private static global::System.Globalization.CultureInfo resourceCulture;
        ///// <summary>
        /////   Overrides the current thread's CurrentUICulture property for all
        /////   resource lookups using this strongly typed resource class.
        ///// </summary>
        //[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        //public static global::System.Globalization.CultureInfo Culture
        //{
        //    get
        //    {
        //        return resourceCulture;
        //    }
        //    set
        //    {
        //        resourceCulture = value;
        //    }
        //}

    public static string getString(string value)
        {
            string res="";
            //if (object.ReferenceEquals(stringsResourceMan, null))
            //{
            //    var temp = new global::System.Resources.ResourceManager("CoreManger.Resources.Strings", typeof(Resources.Strings).Assembly);
            //    stringsResourceMan = temp;
            //}
            try
            {
                res = Strings.ResourceManager.GetString(value);
            }
            catch (Exception e)
            {
                res = "Resource Error";
            }
            return res;
        }
        public static string getMessage(string value)
        {
            string res = "";
            //if (object.ReferenceEquals(messageResourceMan, null))
            //{
            //    System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CoreManger.Resources.Messages", typeof(Resources.Messages).Assembly);
            //    messageResourceMan = temp;
            //}
            try
            {
                res = Messages.ResourceManager.GetString(value);
            }
            catch (Exception e)
            {
                res = "Resource Error";
            }
            return res;
        }

       
    }
}
