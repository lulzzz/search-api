package search.javaoplusd;

import java.util.List;

import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

@SpringBootApplication
@RestController
public class Api
{
	static Db _db;

	@RequestMapping(value = "/exact", method = RequestMethod.GET, produces = "application/json")
	public List<DbMolecule> exact(String smiles, String filter, int skip, int take) throws Exception {
		return _db.exact(smiles, filter, skip, skip + take);
	}

	@RequestMapping(value = "/sub", method = RequestMethod.GET, produces = "application/json")
	public List<DbMolecule> sub(String smiles, String filter, int skip, int take) throws Exception {
		String optimized = Chem.getOptimizedSmarts(smiles);
		return _db.sub(optimized, filter, skip, skip + take);
	}

	@RequestMapping(value = "/sup", method = RequestMethod.GET, produces = "application/json")
	public List<DbMolecule> sup(String smiles, String filter, int skip, int take) throws Exception {
		String optimized = Chem.getOptimizedSmarts(smiles);
		return _db.sup(optimized, filter, skip, skip + take);
	}

	@RequestMapping(value = "/sim", method = RequestMethod.GET, produces = "application/json")
	public List<DbMolecule> similar(String smiles, String filter, int skip, int take) throws Exception {
		// String optimized = Chem.getOptimizedSmarts(smiles);
		return _db.similar(smiles, filter, skip, skip + take);
	}

	// args[0] must be connection string
	public final static void main(String[] args)
	{
		String con = System.getenv("ora_connection");

		if(con == null || con.length() == 0) {
			System.exit(-1);
		}

		_db = new Db(con);

		new SpringApplicationBuilder(Api.class).run(args);
	}

}
