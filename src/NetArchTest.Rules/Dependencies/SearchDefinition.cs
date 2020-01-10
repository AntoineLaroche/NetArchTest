﻿namespace NetArchTest.Rules.Dependencies
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;

    /// <summary>
    /// Manages the parameters and results for a dependency search.
    /// </summary>
    internal class SearchDefinition
    {
        /// <summary> The list of dependencies that has been found in the search. </summary>
        private readonly Dictionary<string, HashSet<string>> _found;

        /// <summary> The list of types that has been checked by the search. </summary>
        private readonly HashSet<string> _checked;

        /// <summary> The list of dependencies being searched for. </summary>
        private readonly IEnumerable<string> _searchList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchDefinition"/> class.
        /// </summary>
        internal SearchDefinition(IEnumerable<string> dependencies)
        {
            _found = new Dictionary<string, HashSet<string>>();
            _checked = new HashSet<string>();
            _searchList = dependencies;
        }

        /// <summary>
        /// Returns all dependencies matching given type full name.
        /// </summary>
        /// <param name="typeFullName"> Type full name for a dependency to match. </param>
        /// <returns> Sequence of all dependencies matching given type full name or empty sequence, if there is no match. </returns>
        internal IEnumerable<string> GetAllMatchingDependencies(string typeFullName)
        {
            return _searchList.Where(d => typeFullName.StartsWith(d));
        }

        /// <summary>
        /// Returns all dependencies matching any of given type full names.
        /// </summary>
        /// <param name="typesFullNames"> Set of type full names for a dependency to match any of them. </param>
        /// <returns> Sequence of all dependencies matching any of given type full names or empty sequence, if there is no match. </returns>
        internal IEnumerable<string> GetAllDependenciesMatchingAnyOf(IEnumerable<string> typesFullNames)
        {
            return _searchList.Where(d => typesFullNames.Any(t => t.StartsWith(d)));
        }

        /// <summary>
        /// Gets the list of dependency names that have been found.
        /// </summary>
        internal IReadOnlyList<string> DependenciesFound
        {
            get
            {
                return _found.Values.SelectMany(t => t).ToArray();
            }
        }

        /// <summary>
        /// Gets the list of types that have dependencies.
        /// </summary>
        internal IReadOnlyList<string> TypesFound
        {
            get
            {
                return _found.Keys.ToArray();
            }
        }

        /// <summary>
        /// Gets an indication of whether a type has been searched.
        /// </summary>
        internal bool IsChecked(TypeDefinition type)
        {
            if (type != null)
            {
                return _checked.Contains(type.FullName);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Adds an item to the list of types that have been searched.
        /// </summary>
        internal void AddToChecked(TypeDefinition type)
        {
            _checked.Add(type.FullName);
        }

        /// <summary>
        /// Adds an item to the list of dependencies that have been found.
        /// </summary>
        internal void AddToFound(TypeDefinition type, string dependency)
        {
            // For private nested types we should treat the parent as the dependency - e.g. async methods are always implemented as private nested classes
            var key = type.FullName;
            while (type.IsNestedPrivate || type == null)
            {
                type = type.DeclaringType;
                if (type != null)
                {
                    key = type.FullName;
                }
            }

            if (_found.ContainsKey(key))
            {
                _found[key].Add(dependency);
            }
            else
            {
                _found.Add(key, new HashSet<string> { dependency });
            }
        }
    }
}
