using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace QueueExchange
{
    class Expression
    {

        /*
         *  Class that contains other expressions and filters. 
         *  Expressions are a either "and" "or" or "simple".
         *  
         *  "simple" expressions contain only a single filter
         *  "and" and "or" expressions cantain any number of other expreesions or filters
         *  which will be logically ANDed or ORed according to their type  to produce a 
         *  boolean result. 
         *  
         *  The attribute not="true" inverts the final output
         */
        private readonly List<Expression> expressions = new List<Expression>();
        private readonly List<IQueueFilter> filters = new List<IQueueFilter>();
        private readonly Expression singleExpression;
        private readonly IQueueFilter singleFilter;
        private readonly QueueFactory filterFact = new QueueFactory();
        private readonly string type = "single";
        public static readonly string[] filterTypes = { "xpexists", "xpmatches", "matches", "xpequals", "dateRange", "bool", "contains", "length", "contextContains" };
        public static readonly string[] expressionTypes = { "and", "or", "not", "xor" };

        public Expression(XElement defn, QueueAbstract queue)
        {

            try
            {
                type = defn.Attribute("type").Value;
            }
            catch (Exception)
            {
                type = "single";
            }

            type = defn.Name.ToString();

            foreach (string fType in filterTypes)
            {
                foreach (XElement filterConfig in defn.Elements(fType))
                {
                    IQueueFilter filter = filterFact.GetFilter(filterConfig, queue);
                    if (filter != null)
                    {
                        filters.Add(filter);
                    }

                    // If it is a "single" type of expression, there will only be one filter Element
                    if (type == "single" || type == "not")
                    {
                        singleFilter = filter;
                    }
                }
            }

            // Boolean operators are all implemented in the this class

            foreach (string eType in expressionTypes)
            {
                foreach (XElement exprConfig in defn.Elements(eType))
                {
                    Expression exp = new Expression(exprConfig, queue);
                    expressions.Add(exp);
                    if (type == "single" || type == "not")
                    {
                        singleExpression = exp;
                    }
                }
            }
        }
        public bool Pass(string message)
        {

            if (type == "not")
            {

                // Return the opposite of the internally evaluated  expression/filter

                if (singleExpression != null)
                {
                    return !singleExpression.Pass(message);
                }
                else if (singleFilter != null)
                {
                    return !singleFilter.Pass(message);
                }

            }
            else if (type == "and")
            {

                //If anyhting false, then return false;

                foreach (Expression exp in expressions)
                {
                    if (!exp.Pass(message))
                    {
                        return false;
                    }
                }
                foreach (IQueueFilter filter in filters)
                {
                    if (!filter.Pass(message))
                    {
                        return false;
                    }
                }
                return true;

            }
            else if (type == "or")
            {

                // Or processing

                bool result = false;

                foreach (Expression exp in expressions)
                {
                    result = result || exp.Pass(message);
                    if (result)
                    {
                        return result;
                    }
                }
                foreach (IQueueFilter filter in filters)
                {
                    result = result || filter.Pass(message);
                    if (result)
                    {
                        return result;
                    }
                }

                return result;

            }
            else if (type == "xor")
            {

                // Or processing

                int count = 0; ;

                foreach (Expression exp in expressions)
                {
                    count = exp.Pass(message) ? count + 1 : count;
                }
                foreach (IQueueFilter filter in filters)
                {
                    count = filter.Pass(message) ? count + 1 : count;
                }

                return count == 1;

            }
            else if (type == "single")
            {
                return singleFilter.Pass(message);
            }

            return false;
        }
    }
}
