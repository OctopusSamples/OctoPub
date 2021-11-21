package com.octopus.octopub.repositories;

import com.github.tennaito.rsql.jpa.JpaPredicateVisitor;
import com.octopus.octopub.Constants;
import com.octopus.octopub.models.Product;
import cz.jirutka.rsql.parser.RSQLParser;
import cz.jirutka.rsql.parser.ast.Node;
import cz.jirutka.rsql.parser.ast.RSQLVisitor;
import java.util.List;
import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;
import javax.persistence.criteria.CriteriaBuilder;
import javax.persistence.criteria.CriteriaQuery;
import javax.persistence.criteria.From;
import javax.persistence.criteria.Predicate;
import lombok.NonNull;
import org.h2.util.StringUtils;

@ApplicationScoped
public class ProductRepository {

  @Inject EntityManager em;

  public Product findOne(final int id) {
    return em.find(Product.class, id);
  }

  public void delete(final int id) {
    em.createQuery("delete from Product p where p.id=:id").setParameter("id", id).executeUpdate();
  }

  public void update(@NonNull final Product product) {
    final Product existingProduct = em.find(Product.class, product.id);
    if (existingProduct != null) {
      existingProduct.name = product.name;
      existingProduct.tenant = product.tenant;
      existingProduct.description = product.description;
      existingProduct.epub = product.epub;
      existingProduct.image = product.epub;
      existingProduct.pdf = product.pdf;
      em.merge(existingProduct);
    }
  }

  public List<Product> findAll(@NonNull final String tenant, final String filter) {

    final CriteriaBuilder builder = em.getCriteriaBuilder();
    final CriteriaQuery<Product> criteria = builder.createQuery(Product.class);
    final From<Product, Product> root = criteria.from(Product.class);

    // add the tenant search rules
    final Predicate tenantPredicate =
        builder.or(
            builder.equal(root.get("tenant"), Constants.DEFAULT_TENANT),
            builder.equal(root.get("tenant"), tenant));

    if (!StringUtils.isNullOrEmpty(filter)) {
      /*
       Makes use of RSQL queries to filter any responses:
       https://github.com/jirutka/rsql-parser
      */
      final RSQLVisitor<Predicate, EntityManager> visitor =
          new JpaPredicateVisitor<Product>().defineRoot(root);
      final Node rootNode = new RSQLParser().parse(filter);
      final Predicate filterPredicate = rootNode.accept(visitor, em);

      // combine with the filter rules
      final Predicate combinedPredicate = builder.and(tenantPredicate, filterPredicate);

      criteria.where(combinedPredicate);
    } else {
      criteria.where(tenantPredicate);
    }

    return em.createQuery(criteria).getResultList();
  }

  public Product save(@NonNull final Product product) {
    product.id = null;
    em.persist(product);
    em.flush();
    return product;
  }
}
