using swatSim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SwopReview
{
	class Reporter
	{
		SwopData _data;
		HtmlGenerator _gtr;

		bool IsCombiMode { get { return _data.WorkMode == SwopWorkMode.COMBI; } }

		public Reporter(SwopData swopData)
		{
			_data = swopData;
		}

		public string CreateTheReport()
		{
			_gtr = new HtmlGenerator();
			_gtr.OpenHtmlFrame();

			AddTitle();

			_gtr.WriteElement("h2", "Instructions");
			AddSetTable();
			AddWorkParameters();

			_gtr.WriteElement("h2", "Statistics");
			if (_data.RunIsCancelled)
				AddUserBreak();

			AddCommonStatistics();
			AddCommonBestParameters();

			AddSetStatistics();
			AddLapStatistics();

			_gtr.WriteElement("h2", "Configuration");

			AddStandardParameters();
			AddIndividualParameters();
			
			_gtr.CloseHtmlFrame();

			return _gtr.Code;
		}

		void AddTitle()
		{
			_gtr.WriteElement("h1","SWOP-Log: "+ _data.SwopLogName);
			
			_gtr.WriteElement("h2", "Mode: "+_data.WorkMode);


			if (!string.IsNullOrWhiteSpace(_data.Description))
				_gtr.WriteElement("h4", _data.Description);
		}

		#region instructions

		void AddSetTable()
		{
			if(IsCombiMode)
				_gtr.WriteElement("h3", "Sets for Parameter-Combination");
			else
			   _gtr.WriteElement("h3", "Sets for Optimization");

			List<string> headers = new List<string> { "Set-No", "Monitoring", "Weather", "Eval-Time", "Eval-Weight", " No.ind.Params" };
			List<int> colWidths = new List<int> { 5, 20, 20, 10, 5, 5};

			List<List<string>> tableRows = new List<List<string>>();
					
			foreach( SwopSet ss in _data.OptSets)
			{
				List<string> row = new List<string>
				{
					ss.SetIndex.ToString(),
					ss.Monitoring,
					ss.Weather,
					ss.EvalTime,
					ss.Weight.ToString("0.##", CultureInfo.InvariantCulture),
					ss.LocalParams.Count.ToString()
				};
				tableRows.Add(row);
			}
			_gtr.WriteTable(headers, tableRows, colWidths);

		}

		void AddWorkParameters()
		{
			char[] delim = new char[] { ' ', '\t' };
			if (IsCombiMode)
				_gtr.WriteElement("h3", "Combine Parameters");
			else
				_gtr.WriteElement("h3", "Optimize Parameters");


			//Dictionary<string, SimParamElem> sd = _data.DefaultParameters.ParamDict;
			List<string> headers = new List<string> { "Param-No", "Param-Key","Description" };
			List<int> colWidths = new List<int> { 5, 20,0 };

			List<List<string>> tableRows = new List<List<string>>();
			int index = 1;
			string desc="";
			string param = "";
			foreach (string p in _data.OptParameters)
			{
				if (IsCombiMode)
				{
					string[] w = p.Split(delim, StringSplitOptions.RemoveEmptyEntries);
					if (w.Count() > 3)
					{ 
						param = w[0];
						string d = _data.DefaultParameters.ParamDict.ContainsKey(param) ?
										_data.DefaultParameters.ParamDict[param].Descr :
									"";
						desc = $"{d}     Min: {w[1]}   Max: {w[2]}  Steps: {w[3]}";
						
					}

				}
				else // opti
				{
					param = p;
					desc = _data.DefaultParameters.ParamDict.ContainsKey(p) ?
						_data.DefaultParameters.ParamDict[p].Descr :
						"";
				}


				List <string> row = new List<string>
				{
					index++.ToString(),
					param,
					desc
				};
				tableRows.Add(row);
			}
			_gtr.WriteTable(headers, tableRows, colWidths);

		}
		#endregion

		#region statistics

		void AddUserBreak()
		{
			_gtr.WriteElement("p", "Optimization canceled by user");
		}

		void AddCommonStatistics()
		{
			_gtr.WriteElement("h3", "Errors / Best Common");
			List<string> headers = new List<string> { "Set-No","Monitoring","Eval-Weight", "Start-Eval", "Eval at Common Best", "Relation" };
			List<int> colWidths = new List<int> { 5,10, 5,10, 10, 0 };

			List<List<string>> tableRows = new List<List<string>>();

			foreach (SwopSet ss in _data.OptSets)
			{
				List<string> row = new List<string>
				{
					ss.SetIndex.ToString(),
					ss.Monitoring,
					ss.Weight.ToString("0.###", CultureInfo.InvariantCulture),
					ss.StartErrValue.ToString("0.###", CultureInfo.InvariantCulture),
					ss.BestErrValue.ToString("0.###", CultureInfo.InvariantCulture),
					(ss.BestErrValue / ss.StartErrValue).ToString("0.00", CultureInfo.InvariantCulture)
				};
				tableRows.Add(row);
			}

			List<string> trow = new List<string>
				{
					"Common",
					" ",
					" ",
					_data.StartCommonError.ToString("0.###", CultureInfo.InvariantCulture),
					_data.BestCommonError.ToString("0.###", CultureInfo.InvariantCulture),
					(_data.BestCommonError / _data.StartCommonError).ToString("0.00", CultureInfo.InvariantCulture)
				};
			tableRows.Add(trow);

			_gtr.WriteTable(headers, tableRows, colWidths);
		}

		public void AddCommonBestParameters()
		{
			//Dictionary<string, SimParamElem> sd = _data.DefaultParameters.ParamDict;

			_gtr.WriteElement("h3", "Best Common Parameters");
			List<string> headers = new List<string> { "Param-No", "Param-Key", "Start-Value", "Best Value" };
			List<int> colWidths = new List<int> {5, 20, 20,0 };

			List<List<string>> tableRows = new List<List<string>>();
			int id = 0;
			foreach(string s in _data.OptParameters)
			{
				string param = s;
				if(IsCombiMode)
				{
					string[] w = s.Split(' ');
					param = w[0];
				}
				List<string> row = new List<string>
				{
					(id+1).ToString(),
					param
				}; 
				row.Add(ParamCreator.GetParameterValueString(_data.DefaultParameters, s, _data.StartParamValues[id]));
				row.Add(ParamCreator.GetParameterValueString(_data.DefaultParameters, s, _data.BestParamValues[id++]));
				tableRows.Add(row);
			}
			_gtr.WriteTable(headers, tableRows, colWidths);
		}


		public void AddSetStatistics()
		{
			_gtr.WriteElement("h3", "Statistics by Sets");

			List<string> headers = new List<string> { "Set-No", "Monitoring", "Start Eval", "Best Eval","Relation","Step","Lap","Best Parameters" };
			List<int> colWidths = new List<int> {5, 20, 5, 5, 5, 5, 5,0 };
			List<List<string>> tableRows = new List<List<string>>();

			foreach (SwopSet ss in _data.OptSets)
			{
				int step = ss.GetBestErrorId();// GetBestErrorId(ss);
				if(step >= 0)
				{ 
					List<string> row = new List<string>
					{
						ss.SetIndex.ToString(),
						ss.Monitoring,
						ss.StartErrValue.ToString("0.###", CultureInfo.InvariantCulture),
						ss.ErrValues[step].ToString("0.###", CultureInfo.InvariantCulture),
						(ss.ErrValues[step]/ss.StartErrValue).ToString("0.00", CultureInfo.InvariantCulture),
						step.ToString(),
						_data.StepLaps[step],
						GetStepParamString(step)
					};
					tableRows.Add(row);
				}
			}

			int bestCommonStep = GetBestStep();
			List<string> rowCom = new List<string>
				{
					"Common",
					"",
					_data.CommonErrorsAbsolute[0].ToString("0.###", CultureInfo.InvariantCulture),
					_data.CommonErrorsAbsolute[bestCommonStep].ToString("0.###", CultureInfo.InvariantCulture),
					(_data.CommonErrorsAbsolute[bestCommonStep]/_data.CommonErrorsAbsolute[0]).ToString("0.00", CultureInfo.InvariantCulture),
					bestCommonStep.ToString(),
					_data.StepLaps[bestCommonStep],
					GetStepParamString(bestCommonStep)
				};
			tableRows.Add(rowCom);

			_gtr.WriteTable(headers, tableRows, colWidths);
		}

		public void AddLapStatistics()
		{
			_gtr.WriteElement("h3", "Statistics by Optimization-Laps");

			List<string> headers = new List<string> { "Lap", "Start-Step", "Num Steps", "Best Eval", "Relation", "Best Parameters" };
			List<int> colWidths = new List<int> { 10, 10, 10,10, 10, 0};
			List<List<string>> tableRows = new List<List<string>>();

			foreach (string lap in _data.LapNames)
			{
				int bestStep = GetBestLapStep(lap);
				List<string> row = new List<string>
				{
					lap,
					GetStartLapStep(lap).ToString(),
					GetNumLapSteps(lap).ToString(),
					_data.CommonErrorsAbsolute[bestStep].ToString("0.###",CultureInfo.InvariantCulture),
					(_data.CommonErrorsAbsolute[bestStep]/ _data.StartCommonError).ToString("0.00",CultureInfo.InvariantCulture),
					GetStepParamString(bestStep)
				};
				tableRows.Add(row);
			}
			
			_gtr.WriteTable(headers, tableRows, colWidths);
		}

		#endregion

		#region configuration

		void AddStandardParameters()
		{
			Dictionary<string, SimParamElem> sd = _data.DefaultParameters.ParamDict;
			var list = sd.Keys.ToList();
			list.Sort();

			_gtr.WriteElement("h3", "Default Model-Parameters");

			List<string> headers = new List<string> { "Parameter-Key", "Value","Description" };
			List<int> colWidths = new List<int> { 20, 10,0 };

			List<List<string>> tableRows = new List<List<string>>();

			foreach (string p in list)
			{
				List<string> row = new List<string>();
				row.Add(p);

				if (sd[p].ObjType == typeof(double))
				{ 
					row.Add($"{((double)(sd[p].Obj)).ToString("0.0###", CultureInfo.InvariantCulture)}");
				}
				else
				{ 
					row.Add($"{((IConvertible)sd[p].Obj).ToString(CultureInfo.InvariantCulture)}");
				}

				row.Add(sd[p].Descr);

				tableRows.Add(row);
			}
			_gtr.WriteTable(headers, tableRows, colWidths);


		}


		void AddIndividualParameters()
		{
			_gtr.WriteElement("h3", "Indiv. Parameter-Sets");

			List<string> headers = new List<string> { "Set-No", "Monitoring", "Weather", "Eval-Time", "Eval-Weight", " Params" };
			List<int> colWidths = new List<int> { 5, 20, 20, 10, 5, 0 };

			List<List<string>> tableRows = new List<List<string>>();

			foreach (SwopSet ss in _data.OptSets)
			{
				List<string> row = new List<string>
				{
					ss.SetIndex.ToString(),
					ss.Monitoring,
					ss.Weather,
					ss.EvalTime,
					ss.Weight.ToString("0.0#", CultureInfo.InvariantCulture),
					GetIndivParamString(ss)
				};
				tableRows.Add(row);
			}
			_gtr.WriteTable(headers, tableRows, colWidths);

		}

		#endregion

		#region helper

		string GetIndivParamString(SwopSet swopSet)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string p in swopSet.LocalParams)
			{
				if (sb.Length > 0)
					sb.Append("<br>");
				sb.Append(p);
			}
			return sb.ToString();

		}

		public static int GetBestErrorId(SwopSet ss)
		{
			double minVal = ss.ErrValues[0];
			int id = 0;

			for(int n=1;n< ss.ErrValues.Length;n++)
			{
				if (ss.ErrValues[n] < minVal)
				{
					id = n;
					minVal = ss.ErrValues[n];
				}
			}
			return id;
		}

		string GetStepParamString(int step)
		{
			StringBuilder sb = new StringBuilder();
			int p = 0;
			foreach(string param in _data.OptParameters)
			{
				if (sb.Length > 0)
					sb.Append("<br>");
				sb.Append($"{param} = {ParamCreator.GetParameterValueString(_data.DefaultParameters, param, _data.OptParamValues[step, p])}");

				p++;
			}

			return sb.ToString();
		}


		int GetStartLapStep(string lapString)
		{
			int n = 0;

			foreach(string lap in _data.StepLaps)
			{
				n++;
				if ( lap == lapString)
					return n;
			}

			return 0;
		}

		int GetNumLapSteps(string lapString)
		{
			int n = 0;

			foreach (string lap in _data.StepLaps)
			{
				if (lap == lapString)
					n++;
			}

			return n;
		}

		int GetBestLapStep(string lapString)
		{
			int step = GetStartLapStep(lapString);

			double minVal = _data.CommonErrorsAbsolute[step];
			int best=step;
			int id = 0;
			foreach (string lap in _data.StepLaps)
			{
				if ((lap == lapString)&& (_data.CommonErrorsAbsolute[id] < minVal))
				{
					best=id;
					minVal = _data.CommonErrorsAbsolute[id];
				}
				id++;
			}

			return best;
		}

		int GetBestStep()
		{

			double minVal = _data.CommonErrorsAbsolute[0];
			int best = 0;
			
			for(int i = 0; i < _data.CommonErrorsAbsolute.Length; i++)
			{
				if (_data.CommonErrorsAbsolute[i] < minVal)
				{
					best = i;
					minVal = _data.CommonErrorsAbsolute[i];
				}
			}

			return best;
		}


		#endregion

	}
}
