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

	public Db(String connectionString) {
		_connectionString = connectionString;
	}

    private Connection getConnection() throws SQLException {
		Properties connectionProps = new Properties();
		connectionProps.put("user", "c$dcischem");
		connectionProps.put("password", "y3wxf1o(PLpt");
		connectionProps.put("allowMultiQueries", "true");
	
		return DriverManager.getConnection(_connectionString, connectionProps);
	}

	private static final String SELECT_QUERY = "SELECT * FROM (select id, smiles, prop_inchikey as idnumber, prop_name as name, mw, prop_logp as logp, h_acc as hba, h_don as hbd, rotbonds as rotb, psa as tpsa, fsp3, effha as hac FROM TABLE(search.get_structuresc_array)) WHERE ROWNUM > %1$d AND ROWNUM <= %2$d";

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
			ResultSet rs = select.executeQuery(String.format(SELECT_QUERY, from, to));
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