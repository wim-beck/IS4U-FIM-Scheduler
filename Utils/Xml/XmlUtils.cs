/**
 * IS4U's Forefront Identity Manager Scheduler is created to schedule automated 
 * run profiles using configuration files on the Synchronization Service.
 * 
 * Copyright (C) 2013 by IS4U (info@is4u.be)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation version 3.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * A full copy of the GNU General Public License can be found 
 * here: http://opensource.org/licenses/gpl-3.0.
 */
using System;
using System.Xml.Linq;

namespace IS4U.Utils.Xml
{
    /// <summary>
    /// Xml utilities.
    /// </summary>
    public static class XmlUtils
    {
        /// <summary>
        /// Get the boolean value of the given xml attribute.
        /// </summary>
        /// <param name="root">Xml root</param>
        /// <param name="attribute">Xml attribute name</param>
        /// <returns>Boolean value</returns>
        public static bool GetAttributeBooleanValue(XElement root, XName attribute)
        {
            if (root != null && root.Attribute(attribute) != null)
            {
                try
                {
                    bool value = Convert.ToBoolean(root.Attribute(attribute).Value);
                    return value;
                }
                catch (FormatException fe)
                {
                    throw new Exception(string.Format("Attribute '{0}' is not a boolean: '{1}'.", attribute, fe.Message));
                }
            }
            throw new Exception(string.Format("Attribute '{0}' is null.", attribute));
        }

        /// <summary>
        /// Get the integer value of the given xml attribute.
        /// </summary>
        /// <param name="root">Xml root</param>
        /// <param name="attribute">Xml attribute name</param>
        /// <returns>Integer value</returns>
        public static int GetAttributeIntegerValue(XElement root, XName attribute)
        {
            if (root != null && root.Attribute(attribute) != null)
            {
                try
                {
                    int value = Convert.ToInt32(root.Attribute(attribute).Value);
                    return value;
                }
                catch (FormatException fe)
                {
                    throw new Exception(string.Format("Attribute '{0}' is not an integer: '{1}'.", attribute, fe.Message));
                }
            }
            throw new Exception(string.Format("Attribute '{0}' is null.", attribute));
        }

        /// <summary>
        /// Get the string value of the given xml attribute.
        /// </summary>
        /// <param name="root">Xml root</param>
        /// <param name="attribute">Xml attribute name</param>
        /// <returns>String value</returns>
        public static string GetAttributeStringValue(XElement root, XName attribute)
        {
            if (root != null && root.Attribute(attribute) != null)
            {
                return root.Attribute(attribute).Value;
            }
            throw new Exception(string.Format("Attribute '{0}' is null.", attribute));
        }

        /// <summary>
        /// Get the string value of the given xml element.
        /// </summary>
        /// <param name="root">Xml root</param>
        /// <param name="element">Xml tag name</param>
        /// <returns>String value</returns>
        public static string GetElementStringValue(XElement root, XName element)
        {
            if (root != null && root.Element(element) != null)
            {
                return root.Element(element).Value;
            }
            throw new Exception(string.Format("Element '{0}' is null.", element));
        }

        /// <summary>
        /// Get the boolean value of the given xml element.
        /// </summary>
        /// <param name="root">Xml root</param>
        /// <param name="element">Xml tag name</param>
        /// <returns>Boolean value</returns>
        public static bool GetElementBooleanValue(XElement root, XName element)
        {
            if (root != null && root.Element(element) != null)
            {
                try
                {
                    bool value = Convert.ToBoolean(root.Element(element).Value);
                    return value;
                }
                catch (FormatException fe)
                {
                    throw new Exception(string.Format("Element '{0}' is not a boolean: '{1}'.", element, fe.Message));
                }
            }
            throw new Exception(string.Format("Element '{0}' is null.", element));
        }
    }
}
