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

	private static final String SELECT_QUERY = "select id, smiles, csc as idnumber, prop_name as name, mw, prop_logp as logp, h_acc as hba, h_don as hbd, rotbonds as rotb, psa as tpsa, fsp3, effha as hac FROM TABLE(search.get_structuresc_array)";
	private static final String RANGE_QUERY = "SELECT * FROM (%s) WHERE ROWNUM > %d AND ROWNUM <= %d";

    private List<DbMolecule> findInOracle(String sp, String smiles, String filter, int from, int to) throws SQLException {
		List<DbMolecule> results = new ArrayList<DbMolecule>();
		Connection c = getConnection();
		CallableStatement call = null;
		Statement select = null;
		try {//exact_search
			call = c.prepareCall(String.format("{call search.%s(?, ?)}", sp));
			call.setString(1, smiles);
			call.setInt(2, 1); // 0 for BB, 1 for SC

			call.execute();

			select = c.createStatement();
			String selectQuery = (filter != null && filter.length() > 0) ? String.format("%s WHERE %s", SELECT_QUERY, filter) : SELECT_QUERY;
			ResultSet rs = select.executeQuery(String.format(RANGE_QUERY, selectQuery, from, to));
			while (rs.next()) {
				DbMolecule mol = new DbMolecule();

				mol.id = rs.getInt("id");
				mol.smiles = rs.getString("smiles");
				mol.idnumber = rs.getString("idnumber");
				mol.name = rs.getString("name");
				mol.mw = rs.getDouble("mw");
				mol.logp = rs.getDouble("logp");
				mol.hba = rs.getInt("hba");
				mol.hbd = rs.getInt("hbd");
				mol.rotb = rs.getInt("rotb");
				mol.tpsa = rs.getDouble("tpsa");
				mol.fsp3 = rs.getDouble("fsp3");
				mol.hac = rs.getInt("hac");

				results.add(mol);
			}
		} finally {
			if (call != null) { call.close(); }
			if (select != null) { select.close(); }
		}
		return results;
	}

	public List<DbMolecule> exact(String smiles, String filter, int skip, int take) throws SQLException {
		return findInOracle("exact_search", smiles, filter, skip, take);
	}

	public List<DbMolecule> sub(String smiles, String filter, int skip, int take) throws SQLException {
		return findInOracle("substructure_search", smiles, filter, skip, take);
	}

	public List<DbMolecule> sup(String smiles, String filter, int skip, int take) throws SQLException {
		throw new UnsupportedOperationException("Superstructure search is not implemented");
	}

	public List<DbMolecule> similar(String smiles, String filter, int skip, int take) throws SQLException {
		return findInOracle("similarity_search", smiles, filter, skip, take);
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