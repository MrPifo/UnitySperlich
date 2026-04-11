using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sperlich.Monitoring {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class MonitorAttribute : Attribute {

		public string CustomFieldName { get; set; }

		public MonitorAttribute() {

		}
		public MonitorAttribute(string customFieldName) {
			CustomFieldName = customFieldName;
		}
	}
}