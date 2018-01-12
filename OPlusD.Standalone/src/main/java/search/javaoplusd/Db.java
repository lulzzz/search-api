package search.javaoplusd;

import java.sql.CallableStatement;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import javax.naming.OperationNotSupportedException;

import chemaxon.util.ArgumentException;

public class Db {

	private final String _connectionString;
	private final String _username;
	private final String _password;

	public Db(String connectionString, String username, String password) {
		_connectionString = connectionString;
		_username = username;
		_password = password;
	}

    private Connection getConnection() throws SQLException {
		Properties connectionProps = new Properties();
		connectionProps.put("user", _username);
		connectionProps.put("password", _password);
	
		return DriverManager.getConnection(_connectionString, connectionProps);
	}

	// private static final String SELECT_QUERY = "select id, smiles, csc as idnumber, prop_name as name, mw, prop_logp as logp, h_acc as hba, h_don as hbd, rotbonds as rotb, psa as tpsa, fsp3, effha as hac FROM TABLE(search.get_structuresc_array)";
	private static final String SELECT_QUERY = "select id, csc as idnumber FROM TABLE(search.%s)";

    private List<Integer> findInOracle(String sp, int targetInd, String targetTable, String smiles, int hitLimit) throws SQLException {
		List<Integer> results = new ArrayList<Integer>();
		Connection c = getConnection();
		CallableStatement call = null;
		Statement select = null;
		try {//exact_search
			String searchQuery = String.format("{call search.%s(?, ?, ?)}", sp);
			call = c.prepareCall(searchQuery);
			call.setString(1, smiles);
			call.setInt(2, targetInd); // 0 for BB, 1 for SC
			call.setInt(3, hitLimit);

			call.execute();

			select = c.createStatement();
			String selectQuery = String.format(SELECT_QUERY, targetTable);
			ResultSet rs = select.executeQuery(selectQuery);
			while (rs.next()) {
				Integer mol = rs.getInt("id");
				results.add(mol);
			}
		} finally {
			if (call != null) { call.close(); }
			if (select != null) { select.close(); }
			if (c != null) { c.close(); }
		}
		return results;
	}

	public static class Targets {
		public static final int BB = 0;
		public static final int SC = 1;
	}

	public static class Types {
		public static final int SUB = 0;
		public static final int SUP = 1;
		public static final int SIM = 2;
	}


	public List<Integer> find(int target, int type, String smiles, int hitLimit) throws Exception {
		String targetTable = 
			target == Targets.BB ? "get_structurebb_array"
			: target == Targets.SC ? "get_structuresc_array"
			: null;
		
		String sp;

		if(type == Types.SUB) {
			sp = "substructure_search";
		}
		else if (type == Types.SUP) {
			throw new OperationNotSupportedException("Superstructure is not supported");
		}
		else if (type == Types.SIM) {
			sp = "similarity_search";
		}
		else {
			throw new ArgumentException();
		}

		return findInOracle(sp, target, targetTable, smiles, hitLimit);
	}
//--------------------------------------------------------------------------------------------------------------------

// procedure substructure_search(
	// pSmi in varchar2, 
	// pType in number default(0), 
	// maxcount in integer default(200), 
	// timelimit in integer default (null));
// procedure superstructure_search(
	// pSmi in varchar2, 
	// pType in number default(0), 
	// maxcount in integer default(200), 
	// timelimit in integer default (null));

	public List<Integer> subSup(String sp, int targetInd, String targetTable, String smiles, int hitLimit, int timeLimit) throws SQLException {
		List<Integer> results = new ArrayList<Integer>();
		Connection c = getConnection();
		CallableStatement call = null;
		Statement select = null;
		try {//exact_search
			String searchQuery = String.format("{call search.%s(?, ?, ?, ?)}", sp);
			call = c.prepareCall(searchQuery);
			call.setString(1, smiles);
			call.setInt(2, targetInd); // 0 for BB, 1 for SC
			call.setInt(3, hitLimit);
			call.setInt(4, timeLimit);

			call.execute();

			select = c.createStatement();
			String selectQuery = String.format(SELECT_QUERY, targetTable);
			ResultSet rs = select.executeQuery(selectQuery);
			while (rs.next()) {
				Integer mol = rs.getInt("id");
				results.add(mol);
			}
		} finally {
			if (call != null) { call.close(); }
			if (select != null) { select.close(); }
			if (c != null) { c.close(); }
		}
		return results;
	}

// procedure similarity_search(
	// pSmi in varchar2, 
	// pType in number default(0), 
	// pTanimoto in number default(0.6), 
	// maxcount in integer default(200));
	public List<Integer> sim(int targetInd, String targetTable, String smiles, int hitLimit, double minSimilarity) throws SQLException {
		List<Integer> results = new ArrayList<Integer>();
		Connection c = getConnection();
		CallableStatement call = null;
		Statement select = null;
		try {//exact_search
			String searchQuery = "{call search.similarity_search(?, ?, ?, ?)}";
			call = c.prepareCall(searchQuery);
			call.setString(1, smiles);
			call.setInt(2, targetInd); // 0 for BB, 1 for SC
			call.setDouble(3, minSimilarity);
			call.setInt(4, hitLimit);

			call.execute();

			select = c.createStatement();
			String selectQuery = String.format(SELECT_QUERY, targetTable);
			ResultSet rs = select.executeQuery(selectQuery);
			while (rs.next()) {
				Integer mol = rs.getInt("id");
				results.add(mol);
			}
		} finally {
			if (call != null) { call.close(); }
			if (select != null) { select.close(); }
			if (c != null) { c.close(); }
		}
		return results;
	}
}

// call search.exact_search('O=C1CNC(=O)N1', 0); -- BB - 0, SC - 1

// call search.substructure_search('NC(=O)c1cnn[nH]1', 0, 1000)
// call search.similarity_search('NC(=O)c1cnn[nH]1', 0)

// call search.identifier_search('BBV-45051060')
// call search.cdid_search(10)

// select * from table(search.get_cdid_array())

// select * from table(search.get_structurebb_array)
// select * from table(search.get_structuresc_array)