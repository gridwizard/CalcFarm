﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NetSerializer
{
	public sealed class TypeData
	{
		public TypeData(ushort typeID, IDynamicTypeSerializer serializer)
		{
			this.TypeID = typeID;
			this.TypeSerializer = serializer;
		}

		public TypeData(ushort typeID, MethodInfo writer, MethodInfo reader)
		{
			this.TypeID = typeID;
			this.WriterMethodInfo = writer;
			this.ReaderMethodInfo = reader;
		}

		public readonly ushort TypeID;
		public bool IsGenerated { get { return this.TypeSerializer != null; } }
		public readonly IDynamicTypeSerializer TypeSerializer;
		public MethodInfo WriterMethodInfo;
		public MethodInfo ReaderMethodInfo;

		public ILGenerator WriterILGen;
		public ILGenerator ReaderILGen;
	}

	public sealed class CodeGenContext
	{
		readonly Dictionary<Type, TypeData> m_typeMap;

		public CodeGenContext(Dictionary<Type, TypeData> typeMap, MethodInfo serializerSwitch, MethodInfo deserializerSwitch)
		{
			m_typeMap = typeMap;
			this.SerializerSwitchMethodInfo = serializerSwitch;
			this.DeserializerSwitchMethodInfo = deserializerSwitch;
		}

		public MethodInfo SerializerSwitchMethodInfo { get; private set; }
		public MethodInfo DeserializerSwitchMethodInfo { get; private set; }

		public MethodInfo GetWriterMethodInfo(Type type)
		{
			return m_typeMap[type].WriterMethodInfo;
		}

		public MethodInfo GetReaderMethodInfo(Type type)
		{
			return m_typeMap[type].ReaderMethodInfo;
		}

		public bool IsGenerated(Type type)
		{
			return m_typeMap[type].IsGenerated;
		}
	}
}
