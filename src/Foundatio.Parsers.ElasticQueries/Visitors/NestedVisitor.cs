﻿using System;
using Foundatio.Parsers.ElasticQueries.Extensions;
using Foundatio.Parsers.LuceneQueries.Extensions;
using Foundatio.Parsers.LuceneQueries.Nodes;
using Foundatio.Parsers.LuceneQueries.Visitors;
using Nest;

namespace Foundatio.Parsers.ElasticQueries.Visitors {
    public class NestedVisitor: ChainableQueryVisitor {
        public override void Visit(GroupNode node, IQueryVisitorContext context) {
            if (String.IsNullOrEmpty(node.Field) || !IsNestedPropertyType(node.GetNameParts(), context)) {
                base.Visit(node, context);
                return;
            }

            node.SetQuery(new NestedQuery { Path = node.GetFullName() });
            base.Visit(node, context);
        }

        public override void Visit(TermNode node, IQueryVisitorContext context) {
            if (!IsNestedPropertyType(node.Field?.Split('.'), context))
                return;

            node.SetQuery(new NestedQuery { Path = node.GetParentFullName(), Query = node.GetQuery() });
        }

        private bool IsNestedPropertyType(string[] nameParts, IQueryVisitorContext context) {
            var elasticContext = context as IElasticQueryVisitorContext;

            if (nameParts == null || elasticContext == null || nameParts.Length == 0)
                return false;

            string fieldName = String.Empty;
            for (int i = 0; i < nameParts.Length; i++) {
                if (i > 0)
                    fieldName += ".";

                fieldName += nameParts[i];

                if (elasticContext.IsNestedPropertyType(fieldName))
                    return true;
            }

            return false;
        }
    }
}
