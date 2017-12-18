package search.javaoplusd;

import java.util.List;

import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

@SpringBootApplication
@RestController
public class Api
{
	static Db _db;

	@RequestMapping(value = "/", method = RequestMethod.GET)
	public String hello() {
		return "Hello world!";
	}

	@RequestMapping(value = "/sub/{target}", method = RequestMethod.GET, produces = "application/json")
	public List<Integer> sub(@PathVariable String target, String smiles, int hitLimit) throws Exception {
		String optimized = Chem.getOptimizedSmarts(smiles);
		int targetInd = targetToIndex(target);
		String targetTable = targetToTable(target);
		return _db.subSup("substructure_search", targetInd, targetTable, optimized, hitLimit, 10);
	}

	@RequestMapping(value = "/sup/{target}", method = RequestMethod.GET, produces = "application/json")
	public List<Integer> sup(@PathVariable String target, String smiles, int hitLimit) throws Exception {
		String optimized = Chem.getOptimizedSmarts(smiles);
		int targetInd = targetToIndex(target);
		String targetTable = targetToTable(target);
		return _db.subSup("superstructure_search", targetInd, targetTable, optimized, hitLimit, 10);
	}

	@RequestMapping(value = "/sim/{target}", method = RequestMethod.GET, produces = "application/json")
	public List<Integer> similar(@PathVariable String target, String smiles, int hitLimit, double minSimilarity) throws Exception {
		String optimized = Chem.getOptimizedSmarts(smiles);
		int targetInd = targetToIndex(target);
		String targetTable = targetToTable(target);
		return _db.sim(targetInd, targetTable, optimized, hitLimit, minSimilarity);
	}

	private int targetToIndex(String target) throws Exception {
		if("sc".equalsIgnoreCase(target)) return 1;
		if("bb".equalsIgnoreCase(target)) return 0;
		throw new Exception("bad target in route, must be 'sc' or 'bb'");
	}

	private String targetToTable(String target) throws Exception {
		if("sc".equalsIgnoreCase(target)) return "get_structuresc_array";
		if("bb".equalsIgnoreCase(target)) return "get_structurebb_array";
		throw new Exception("bad target in route, must be 'sc' or 'bb'");
	}

	public final static void main(String[] args)
	{
		String con = System.getenv("ora_connection");
		String user = System.getenv("ora_user"); // c$dcischem
		String pass = System.getenv("ora_pass"); // y3wxf1o(PLpt

		System.out.println(con);
		System.out.println(user);
		System.out.println(pass);

		if(con == null || con.length() == 0
		|| user == null || user.length() == 0
		|| pass == null || pass.length() == 0) {
			System.exit(-1);
		}

		_db = new Db(con, user, pass);

		new SpringApplicationBuilder(Api.class).run(args);
	}

}
